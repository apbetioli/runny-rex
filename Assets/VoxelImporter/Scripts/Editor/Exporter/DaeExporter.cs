using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using VoxelImporter.grendgine_collada;

namespace VoxelImporter
{
    public class DaeExporter
    {
        public bool Export(string path, List<Transform> transforms)
        {
            exportedFiles.Clear();

            Grendgine_Collada gCollada = new Grendgine_Collada();

            Func<UnityEngine.Object, string> MakeID = (o) =>
            {
                var id = o.GetInstanceID().ToString();
                return id.Replace('-', 'n');
            };
            Func<Transform, Mesh> MeshFromTransform = (t) =>
            {
                #region SkinnedMeshRenderer
                {
                    var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null && skinnedMeshRenderer.sharedMaterials != null)
                        return skinnedMeshRenderer.sharedMesh;
                }
                #endregion
                #region MeshFilter
                {
                    var meshFilter = t.GetComponent<MeshFilter>();
                    var meshRenderer = t.GetComponent<MeshRenderer>();
                    if (meshFilter != null && meshFilter.sharedMesh != null && meshRenderer != null && meshRenderer.sharedMaterials != null)
                        return meshFilter.sharedMesh;
                }
                #endregion
                return null;
            };
            Func<Transform, Material[]> MaterialsFromTransform = (t) =>
            {
                #region SkinnedMeshRenderer
                {
                    var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null && skinnedMeshRenderer.sharedMaterials != null)
                        return skinnedMeshRenderer.sharedMaterials;
                }
                #endregion
                #region MeshFilter
                {
                    var meshFilter = t.GetComponent<MeshFilter>();
                    var meshRenderer = t.GetComponent<MeshRenderer>();
                    if (meshFilter != null && meshFilter.sharedMesh != null && meshRenderer != null && meshRenderer.sharedMaterials != null)
                        return meshRenderer.sharedMaterials;
                }
                #endregion
                return null;
            };
            Action<Action<Transform, Mesh, Material[]>> MakeFromTransform = (action) =>
            {
                foreach (var t in transforms)
                {
                    #region SkinnedMeshRenderer
                    {
                        var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null && skinnedMeshRenderer.sharedMaterials != null)
                        {
                            action(t, skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
                            continue;
                        }
                    }
                    #endregion
                    #region MeshFilter
                    {
                        var meshFilter = t.GetComponent<MeshFilter>();
                        var meshRenderer = t.GetComponent<MeshRenderer>();
                        if (meshFilter != null && meshFilter.sharedMesh != null && meshRenderer != null && meshRenderer.sharedMaterials != null)
                        {
                            action(t, meshFilter.sharedMesh, meshRenderer.sharedMaterials);
                            continue;
                        }
                    }
                    #endregion
                }
            };

            Matrix4x4 matMirrorX = Matrix4x4.identity;
            matMirrorX.m00 = -matMirrorX.m00;

            const string progressTitle = "Exporting Collada(dae) File...";
            int progressTotal = 11;
            int progressIndex = 0;
            EditorUtility.DisplayProgressBar(progressTitle, "", (progressIndex++ / (float)progressTotal));

            #region Header
            {
                gCollada.Collada_Version = "1.4.1";     //for Blender

                gCollada.Asset = new Grendgine_Collada_Asset()
                {
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    Contributor = new Grendgine_Collada_Asset_Contributor[]
                    {
                        new Grendgine_Collada_Asset_Contributor()
                        {
                            Authoring_Tool = "Voxel Importer",
                            Comments = "https://www.assetstore.unity3d.com/#!/content/62914",
                        },
                    },
                    Revision = "1.0",
                    Title = Path.GetFileNameWithoutExtension(path),
                };
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Header", (progressIndex++ / (float)progressTotal));

            #region Images
            bool singleImage;
            {
                var texList = new HashSet<Texture2D>();
                MakeFromTransform((t, mesh, materials) =>
                {
                    foreach (var material in materials)
                    {
                        if (material.mainTexture != null && material.mainTexture is Texture2D)
                            texList.Add(material.mainTexture as Texture2D);
                    }
                });
                singleImage = texList.Count <= 1;
            }
            var imagesDic = new Dictionary<Texture, Grendgine_Collada_Image>();
            {
                var li = gCollada.Library_Images = new Grendgine_Collada_Library_Images()
                {
                    ID = "Images_" + MakeID(transforms[0].gameObject),
                    Name = "Images_" + transforms[0].gameObject.name,
                };

                Func<Texture2D, string> ExportTexture = (tex) =>
                {
                    const string EXT = ".png";
                    string texpath;
                    if (singleImage)
                        texpath = path.Remove(path.Length - EXT.Length, EXT.Length) + EXT;
                    else
                        texpath = string.Format("{0}/{1}_tex{2}{3}", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), imagesDic.Count, EXT);
                    if (AssetDatabase.Contains(tex) && AssetDatabase.IsMainAsset(tex))
                    {
                        var assetPath = Application.dataPath + AssetDatabase.GetAssetPath(tex).Remove(0, "Assets".Length);
                        var ext = Path.GetExtension(assetPath);
                        if (ext != EXT)
                            texpath = path.Remove(path.Length - EXT.Length, EXT.Length) + ext;
                        File.Copy(assetPath, texpath, true);
                    }
                    else
                    {
                        File.WriteAllBytes(texpath, tex.EncodeToPNG());
                    }
                    exportedFiles.Add(texpath);
                    return Path.GetFileName(texpath);
                };

                MakeFromTransform((t, mesh, materials) =>
                {
                    foreach (var material in materials)
                    {
                        if (material.mainTexture == null || !(material.mainTexture is Texture2D))
                        {
                            Debug.LogWarningFormat("<color=green>[Voxel Importer]</color> Material texture not found. material : {0}", material.name);
                            continue;
                        }
                        var tex2D = material.mainTexture as Texture2D;
                        if (imagesDic.ContainsKey(tex2D)) continue;
                        var image = new Grendgine_Collada_Image()
                        {
                            ID = "Image_" + MakeID(tex2D),
                            Name = tex2D.name,
                            Init_From = Uri.EscapeDataString(ExportTexture(tex2D)),
                        };
                        imagesDic.Add(tex2D, image);
                    }
                });
                li.Image = imagesDic.Values.ToArray();
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Images", (progressIndex++ / (float)progressTotal));

            #region Effects
            var effectsDic = new Dictionary<Material, Grendgine_Collada_Effect>();
            {
                var le = gCollada.Library_Effects = new Grendgine_Collada_Library_Effects()
                {
                    ID = "Effects_" + MakeID(transforms[0].gameObject),
                    Name = "Effects_" + transforms[0].gameObject.name,
                };
                MakeFromTransform((t, mesh, materials) =>
                {
                    foreach (var material in materials)
                    {
                        if (material.mainTexture == null || !(material.mainTexture is Texture2D)) continue;
                        var tex2D = material.mainTexture as Texture2D;
                        if (!imagesDic.ContainsKey(tex2D)) continue;
                        if (effectsDic.ContainsKey(material)) continue;
                        Grendgine_Collada_New_Param surfaceParam = new Grendgine_Collada_New_Param()
                        {
                            sID = "Surface_" + MakeID(material),
                            Surface = new Grendgine_Collada_Surface_1_4_1()
                            {
                                Type = Grendgine_Collada_FX_Surface_Type._2D,
                                Init_From = imagesDic[tex2D].ID,
                            },
                        };
                        Grendgine_Collada_New_Param sampler2DParam = new Grendgine_Collada_New_Param()
                        {
                            sID = "Sampler2D_" + MakeID(material),
                            Sampler2D = new Grendgine_Collada_Sampler2D()
                            {
                                Source = surfaceParam.sID,
                            },
                        };

                        var e = new Grendgine_Collada_Effect()
                        {
                            ID = "Effect_" + MakeID(material),
                            Name = material.name,
                            Profile_COMMON = new Grendgine_Collada_Profile_COMMON[]
                            {
                                new Grendgine_Collada_Profile_COMMON()
                                {
                                    ID = "Profile_" + MakeID(material),
                                    New_Param = new Grendgine_Collada_New_Param[]
                                    {
                                        surfaceParam,
                                        sampler2DParam,
                                    },
                                    Technique = new Grendgine_Collada_Effect_Technique_COMMON()
                                    {
                                        sID = "Technique_" + MakeID(material),
                                        Phong = new Grendgine_Collada_Phong()
                                        {
                                            Emission = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type()
                                            {
                                                Color = new Grendgine_Collada_Color()
                                                {
                                                    sID = "emission",
                                                    Value_As_String = "0 0 0 1",
                                                },
                                            },
                                            Ambient = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type()
                                            {
                                                Color = new Grendgine_Collada_Color()
                                                {
                                                    sID = "ambient",
                                                    Value_As_String = "1 1 1 1",
                                                },
                                            },
                                            Diffuse = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type()
                                            {
                                                Texture = new Grendgine_Collada_Texture()
                                                {
                                                    Texture = sampler2DParam.sID,
                                                },
                                            },
                                            Specular = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type()
                                            {
                                                Color = new Grendgine_Collada_Color()
                                                {
                                                    sID = "specular",
                                                    Value_As_String = "0.5 0.5 0.5 1",
                                                },
                                            },
                                            Shininess = new Grendgine_Collada_FX_Common_Float_Or_Param_Type()
                                            {
                                                Float = new Grendgine_Collada_SID_Float()
                                                {
                                                    sID = "shininess",
                                                    Value = 50,
                                                },
                                            },
                                            Index_Of_Refraction = new Grendgine_Collada_FX_Common_Float_Or_Param_Type()
                                            {
                                                Float = new Grendgine_Collada_SID_Float()
                                                {
                                                    sID = "index_of_refraction",
                                                    Value = 1,
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                        };
                        effectsDic.Add(material, e);
                    }
                });
                le.Effect = effectsDic.Values.ToArray();
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Effects", (progressIndex++ / (float)progressTotal));

            #region Materials
            var materialsDic = new Dictionary<Material, Grendgine_Collada_Material>();
            {
                var lm = gCollada.Library_Materials = new Grendgine_Collada_Library_Materials()
                {
                    ID = "Materials_" + MakeID(transforms[0].gameObject),
                    Name = "Materials_" + transforms[0].gameObject.name,
                };
                MakeFromTransform((t, mesh, materials) =>
                {
                    foreach (var material in materials)
                    {
                        if (!effectsDic.ContainsKey(material)) continue;
                        if (materialsDic.ContainsKey(material)) continue;
                        var effect = effectsDic[material];
                        var m = new Grendgine_Collada_Material()
                        {
                            ID = "Material_" + MakeID(material),
                            Name = material.name,
                            Instance_Effect = new Grendgine_Collada_Instance_Effect()
                            {
                                URL = "#" + effect.ID,
                            },
                        };
                        materialsDic.Add(material, m);
                    }
                });
                lm.Material = materialsDic.Values.ToArray();
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Materials", (progressIndex++ / (float)progressTotal));

            #region Geometries
            bool makeJoint = false;
            var geometriesDic = new Dictionary<Transform, Grendgine_Collada_Geometry>();
            {
                var lg = gCollada.Library_Geometries = new Grendgine_Collada_Library_Geometries()
                {
                    ID = "Geometries_" + MakeID(transforms[0].gameObject),
                    Name = "Geometries_" + transforms[0].gameObject.name,
                };
                MakeFromTransform((t, mesh, materials) =>
                {
                    if (mesh.boneWeights.Length > 0)
                        makeJoint = true;

                    #region Source
                    #region Vertex
                    Grendgine_Collada_Source vertexSource;
                    {
                        Grendgine_Collada_Float_Array array;
                        {
                            var sb = new StringBuilder();
                            foreach (var v in mesh.vertices)
                            {
                                var mv = matMirrorX.MultiplyPoint3x4(v);
                                sb.AppendFormat("\n{0} {1} {2}", mv.x, mv.y, mv.z);
                            }
                            array = new Grendgine_Collada_Float_Array()
                            {
                                ID = "VertexArray_" + MakeID(mesh),
                                Name = mesh.name + "_vertex",
                                Count = mesh.vertexCount * 3,
                                Value_As_String = sb.ToString(),
                            };
                        }
                        vertexSource = new Grendgine_Collada_Source()
                        {
                            ID = "VertexSource_" + MakeID(mesh),
                            Name = mesh.name + "_vertex",
                            Float_Array = array,
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Count = (uint)mesh.vertexCount,
                                    Source = "#" + array.ID,
                                    Stride = 3,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param() { Name = "X", Type = "float", },
                                        new Grendgine_Collada_Param() { Name = "Y", Type = "float", },
                                        new Grendgine_Collada_Param() { Name = "Z", Type = "float", },
                                    },
                                },
                            },
                        };
                    }
                    #endregion
                    #region UV
                    Grendgine_Collada_Source uvSource;
                    {
                        Grendgine_Collada_Float_Array array;
                        {
                            var sb = new StringBuilder();
                            foreach (var uv in mesh.uv)
                                sb.AppendFormat("\n{0} {1}", uv.x, uv.y);
                            array = new Grendgine_Collada_Float_Array()
                            {
                                ID = "UVArray_" + MakeID(mesh),
                                Name = mesh.name + "_uv",
                                Count = mesh.vertexCount * 2,
                                Value_As_String = sb.ToString(),
                            };
                        }
                        uvSource = new Grendgine_Collada_Source()
                        {
                            ID = "UVSource_" + MakeID(mesh),
                            Name = mesh.name + "_uv",
                            Float_Array = array,
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Count = (uint)mesh.vertexCount,
                                    Source = "#" + array.ID,
                                    Stride = 2,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param() { Name = "S", Type = "float", },
                                        new Grendgine_Collada_Param() { Name = "T", Type = "float", },
                                    },
                                },
                            },
                        };
                    }
                    #endregion
                    #region Normal
                    Grendgine_Collada_Source normalSource;
                    {
                        Grendgine_Collada_Float_Array array;
                        {
                            var sb = new StringBuilder();
                            foreach (var n in mesh.normals)
                            {
                                var mn = matMirrorX.MultiplyPoint3x4(n);
                                sb.AppendFormat("\n{0} {1} {2}", mn.x, mn.y, mn.z);
                            }
                            array = new Grendgine_Collada_Float_Array()
                            {
                                ID = "NormalArray_" + MakeID(mesh),
                                Name = mesh.name + "_normal",
                                Count = mesh.vertexCount * 3,
                                Value_As_String = sb.ToString(),
                            };
                        }
                        normalSource = new Grendgine_Collada_Source()
                        {
                            ID = "NormalSource_" + MakeID(mesh),
                            Name = mesh.name + "_normal",
                            Float_Array = array,
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Count = (uint)mesh.vertexCount,
                                    Source = "#" + array.ID,
                                    Stride = 3,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param() { Name = "X", Type = "float", },
                                        new Grendgine_Collada_Param() { Name = "Y", Type = "float", },
                                        new Grendgine_Collada_Param() { Name = "Z", Type = "float", },
                                    },
                                },
                            },
                        };
                    }
                    #endregion
                    #endregion

                    #region Vertices
                    Grendgine_Collada_Vertices vertices;
                    {
                        vertices = new Grendgine_Collada_Vertices()
                        {
                            ID = "Vertices_" + MakeID(mesh),
                            Name = mesh.name + "_vertices",
                            Input = new Grendgine_Collada_Input_Unshared[]
                            {
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.POSITION,
                                    source = "#" + vertexSource.ID,
                                },
                            },
                        };
                    }
                    #endregion

                    #region Triangles
                    Grendgine_Collada_Triangles[] triangles;
                    {
                        triangles = new Grendgine_Collada_Triangles[mesh.subMeshCount];
                        for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
                        {
                            if (mesh.GetTopology(subMesh) != MeshTopology.Triangles)
                            {
                                Debug.LogWarningFormat("<color=green>[Voxel Importer]</color> MeshTopology is not Triangles. Mesh = {0} - {1}, MeshTopology = {1}", mesh.name, subMesh, mesh.GetTopology(subMesh));
                                continue;
                            }
                            if(!materialsDic.ContainsKey(materials[subMesh]))
                                continue;
                            var material = materialsDic[materials[subMesh]];
                            var ts = mesh.GetTriangles(subMesh);
                            var sb = new StringBuilder();
                            {
                                for (int i = 0; i < ts.Length; i += 3)
                                    sb.AppendFormat("\n{0} {0} {0} {1} {1} {1} {2} {2} {2}", ts[i + 0], ts[i + 2], ts[i + 1]);
                            }
                            triangles[subMesh] = new Grendgine_Collada_Triangles()
                            {
                                Count = ts.Length / 3,
                                Name = mesh.name + "_triangles",
                                Material = material.ID,
                                Input = new Grendgine_Collada_Input_Shared[]
                                {
                                    new Grendgine_Collada_Input_Shared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.VERTEX,
                                        source = "#" + vertices.ID,
                                        Offset = 0,
                                    },
                                    new Grendgine_Collada_Input_Shared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD,
                                        source = "#" + uvSource.ID,
                                        Offset = 1,
                                    },
                                    new Grendgine_Collada_Input_Shared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.NORMAL,
                                        source = "#" + normalSource.ID,
                                        Offset = 2,
                                    },
                                },
                                P = new Grendgine_Collada_Int_Array_String()
                                {
                                    Value_As_String = sb.ToString(),
                                },
                            };
                        }
                    }
                    #endregion

                    var g = new Grendgine_Collada_Geometry()
                    {
                        ID = "Geometry_" + MakeID(mesh),
                        Name = mesh.name,
                        Mesh = new Grendgine_Collada_Mesh()
                        {
                            Source = new Grendgine_Collada_Source[]
                            {
                                vertexSource,
                                uvSource,
                                normalSource,
                            },
                            Vertices = vertices,
                            Triangles = triangles,
                        },
                    };
                    geometriesDic.Add(t, g);
                });
                lg.Geometry = geometriesDic.Values.ToArray();
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Geometries", (progressIndex++ / (float)progressTotal));

            #region Nodes
            var nodesDic = new Dictionary<Transform, Grendgine_Collada_Node>();
            {
                Func<Transform, Grendgine_Collada_Node> MakeNode = null;
                MakeNode = (t) =>
                {
                    var node = new Grendgine_Collada_Node()
                    {
                        ID = "Node_" + MakeID(t),
                        Name = t.name,
                        sID = "Node_" + MakeID(t),
                        Type = Grendgine_Collada_Node_Type.NODE,
                    };
                    if (t != transforms[0])
                    {
                        var mat = Matrix4x4.TRS(matMirrorX.MultiplyPoint3x4(t.localPosition),
                                                new Quaternion(t.localRotation.x, -t.localRotation.y, -t.localRotation.z, t.localRotation.w), //mirrorX
                                                t.localScale);
                        var sb = new StringBuilder();
                        for (int r = 0; r < 4; r++)
                            for (int c = 0; c < 4; c++)
                                sb.AppendFormat("{0} ", mat[r, c]);
                        sb.Remove(sb.Length - 1, 1);
                        node.Matrix = new Grendgine_Collada_Matrix[]
                        {
                            new Grendgine_Collada_Matrix()
                            {
                                Value_As_String = sb.ToString(),
                            },
                        };
                    }
                    {
                        List<Grendgine_Collada_Node> nodes = new List<Grendgine_Collada_Node>();
                        for (int i = 0; i < t.childCount; i++)
                        {
                            var ct = t.GetChild(i);
                            if (!transforms.Contains(ct)) continue;
                            var n = MakeNode(ct);
                            nodes.Add(n);
                            nodesDic.Add(ct, n);
                        }
                        node.node = nodes.ToArray();
                    }
                    if (geometriesDic.ContainsKey(t))
                    {
                        var mesh = MeshFromTransform(t);
                        if (mesh == null) return node;
                        var materials = MaterialsFromTransform(t);
                        if (materials == null) return node;
                        var Instance_Material = new Grendgine_Collada_Instance_Material_Geometry[materials.Length];
                        for (int j = 0; j < materials.Length; j++)
                        {
                            if (!materialsDic.ContainsKey(materials[j])) continue;
                            var mat = materialsDic[materials[j]];
                            Instance_Material[j] = new Grendgine_Collada_Instance_Material_Geometry()
                            {
                                Target = "#" + mat.ID,
                                Symbol = mat.ID,
                            };
                        }
                        node.Instance_Geometry = new Grendgine_Collada_Instance_Geometry[]
                        {
                        new Grendgine_Collada_Instance_Geometry()
                        {
                            URL = "#" + geometriesDic[t].ID,
                            Bind_Material = new Grendgine_Collada_Bind_Material[]
                            {
                                new Grendgine_Collada_Bind_Material()
                                {
                                    Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material()
                                    {
                                        Instance_Material = Instance_Material,
                                    },
                                },
                            },
                        },
                        };
                    }
                    return node;
                };
                nodesDic.Add(transforms[0], MakeNode(transforms[0]));
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Nodes", (progressIndex++ / (float)progressTotal));

            #region Joints
            var jointsDic = new Dictionary<Transform, Grendgine_Collada_Node>();
            if (makeJoint)
            {
                Func<Transform, Grendgine_Collada_Node> MakeJoint = null;
                MakeJoint = (t) =>
                {
                    var Doc = new System.Xml.XmlDocument();
                    var Data = new System.Xml.XmlElement[]
                    {
                        Doc.CreateElement("tip_x"),
                        Doc.CreateElement("tip_y"),
                        Doc.CreateElement("tip_z"),
                    };
                    {
                        var offset = new Vector3(0, 0, 0.0001f);
                        if (t.childCount > 0)
                            offset = matMirrorX.MultiplyVector(t.GetChild(0).localPosition);
                        else
                            offset = matMirrorX.MultiplyVector(t.localPosition).normalized * t.localPosition.magnitude * 0.5f;
                        Data[0].InnerText = offset.x.ToString();
                        Data[1].InnerText = offset.y.ToString();
                        Data[2].InnerText = offset.z.ToString();
                    }
                    var joint = new Grendgine_Collada_Node()
                    {
                        ID = "Joint_" + MakeID(t),
                        Name = t.name,
                        sID = "Joint_" + MakeID(t),
                        Type = Grendgine_Collada_Node_Type.JOINT,
                        Matrix = nodesDic[t].Matrix,
                        Extra = new Grendgine_Collada_Extra[]
                        {
                            new Grendgine_Collada_Extra()
                            {
                                Technique = new Grendgine_Collada_Technique[]
                                {
                                    new Grendgine_Collada_Technique()
                                    {
                                        profile = "blender",
                                        Data = Data,
                                    },
                                },
                            },
                        },
                    };
                    List<Grendgine_Collada_Node> joints = new List<Grendgine_Collada_Node>();
                    for (int i = 0; i < t.childCount; i++)
                    {
                        var ct = t.GetChild(i);
                        if (!transforms.Contains(ct)) continue;
                        var n = MakeJoint(ct);
                        joints.Add(n);
                        jointsDic.Add(ct, n);
                    }
                    joint.node = joints.ToArray();
                    return joint;
                };
                jointsDic.Add(transforms[0], MakeJoint(transforms[0]));
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Joints", (progressIndex++ / (float)progressTotal));

            #region Controllers
            var controllersDic = new Dictionary<Transform, Grendgine_Collada_Controller>();
            {
                var lc = gCollada.Library_Controllers = new Grendgine_Collada_Library_Controllers()
                {
                    ID = "Controllers_" + MakeID(transforms[0].gameObject),
                    Name = "Controllers_" + transforms[0].gameObject.name,
                };
                
                foreach (var t in transforms)
                {
                    var skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null || skinnedMeshRenderer.sharedMaterials == null)
                        continue;
                    if (skinnedMeshRenderer.sharedMesh.boneWeights.Length <= 0)
                        continue;
                    var boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

                    #region Joints_Source
                    Grendgine_Collada_Source Joints_Source; 
                    {
                        var Joints_Name_Array = new Grendgine_Collada_Name_Array()
                        {
                            ID = "Joints_Name_Array_" + MakeID(t),
                            Count = skinnedMeshRenderer.bones.Length,
                        };
                        {
                            var names = new StringBuilder();
                            foreach (var bone in skinnedMeshRenderer.bones)
                            {
                                names.AppendFormat("\n{0}", jointsDic[bone].ID);
                            }
                            Joints_Name_Array.Value_Pre_Parse = names.ToString();
                        }
                        Joints_Source = new Grendgine_Collada_Source()
                        {
                            ID = "Joints_" + MakeID(t),
                            Name_Array = Joints_Name_Array,
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Count = (uint)Joints_Name_Array.Count,
                                    Source = "#" + Joints_Name_Array.ID,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Type = "name",
                                        },
                                    },
                                },
                            },
                        };
                    }
                    #endregion
                    #region Weights_Source
                    Grendgine_Collada_Source Weights_Source;
                    int[] weightsCount = new int[boneWeights.Length];
                    StringBuilder weightsVCountString = new StringBuilder();
                    StringBuilder weightsVString = new StringBuilder();
                    List<float> weightList = new List<float>();
                    {
                        var Weights_Float_Array = new Grendgine_Collada_Float_Array()
                        {
                            ID = "Weights_Float_Array_" + MakeID(t),
                        };
                        {
                            var sb = new StringBuilder();
                            for (int i = 0; i < boneWeights.Length; i++)
                            {
                                int count = 0;
                                {
                                    if (!weightList.Contains(boneWeights[i].weight0))
                                    {
                                        weightList.Add(boneWeights[i].weight0);
                                        sb.AppendFormat("\n{0}", boneWeights[i].weight0);
                                    }
                                    weightsVString.AppendFormat("\n{0} {1}", boneWeights[i].boneIndex0, weightList.IndexOf(boneWeights[i].weight0));
                                    count++;
                                }
                                if (boneWeights[i].weight1 > 0f)
                                {
                                    if (!weightList.Contains(boneWeights[i].weight1))
                                    {
                                        weightList.Add(boneWeights[i].weight1);
                                        sb.AppendFormat("\n{0}", boneWeights[i].weight1);
                                    }
                                    weightsVString.AppendFormat(" {0} {1}", boneWeights[i].boneIndex1, weightList.IndexOf(boneWeights[i].weight1));
                                    count++;
                                }
                                if (boneWeights[i].weight2 > 0f)
                                {
                                    if (!weightList.Contains(boneWeights[i].weight2))
                                    {
                                        weightList.Add(boneWeights[i].weight2);
                                        sb.AppendFormat("\n{0}", boneWeights[i].weight2);
                                    }
                                    weightsVString.AppendFormat(" {0} {1}", boneWeights[i].boneIndex2, weightList.IndexOf(boneWeights[i].weight2));
                                    count++;
                                }
                                if (boneWeights[i].weight3 > 0f)
                                {
                                    if (!weightList.Contains(boneWeights[i].weight3))
                                    {
                                        weightList.Add(boneWeights[i].weight3);
                                        sb.AppendFormat("\n{0}", boneWeights[i].weight3);
                                    }
                                    weightsVString.AppendFormat(" {0} {1}", boneWeights[i].boneIndex3, weightList.IndexOf(boneWeights[i].weight3));
                                    count++;
                                }
                                weightsCount[i] = count;
                                weightsVCountString.AppendFormat("\n{0}", count);
                            }
                            Weights_Float_Array.Count = weightList.Count;
                            Weights_Float_Array.Value_As_String = sb.ToString();
                        }
                        Weights_Source = new Grendgine_Collada_Source()
                        {
                            ID = "Weights_" + MakeID(t),
                            Float_Array = Weights_Float_Array,
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Count = (uint)Weights_Float_Array.Count,
                                    Source = "#" + Weights_Float_Array.ID,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Type = "float",
                                        },
                                    },
                                },
                            },
                        };
                    }
                    #endregion
                    #region Inv_Bind_Mats_Source
                    Grendgine_Collada_Source Inv_Bind_Mats_Source;
                    {
                        var Inv_Bind_Mats_Float_Array = new Grendgine_Collada_Float_Array()
                        {
                            ID = "Inv_Bind_Mats_" + MakeID(t),
                        };
                        {
                            var bindposes = skinnedMeshRenderer.sharedMesh.bindposes;
                            var sb = new StringBuilder();
                            for (int i = 0; i < bindposes.Length; i++)
                            {
                                Matrix4x4 mat;
                                {
                                    var position = bindposes[i].GetColumn(3);
                                    var rotation = Quaternion.LookRotation(bindposes[i].GetColumn(2), bindposes[i].GetColumn(1));
                                    var scale = new Vector3(bindposes[i].GetColumn(0).magnitude, bindposes[i].GetColumn(1).magnitude, bindposes[i].GetColumn(2).magnitude);
                                    mat = Matrix4x4.TRS(matMirrorX.MultiplyPoint3x4(position),
                                                        new Quaternion(rotation.x, -rotation.y, -rotation.z, rotation.w), //mirrorX
                                                        scale);
                                }
                                for (int r = 0; r < 4; r++)
                                    sb.AppendFormat("\n{0} {1} {2} {3}", mat[r, 0], mat[r, 1], mat[r, 2], mat[r, 3]);
                            }
                            Inv_Bind_Mats_Float_Array.Count = bindposes.Length * 16;
                            Inv_Bind_Mats_Float_Array.Value_As_String = sb.ToString();
                        }
                        Inv_Bind_Mats_Source = new Grendgine_Collada_Source()
                        {
                            ID = "Inv_Bind_Mats_" + MakeID(t),
                            Float_Array = Inv_Bind_Mats_Float_Array,
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Count = (uint)(Inv_Bind_Mats_Float_Array.Count / 16),
                                    Source = "#" + Inv_Bind_Mats_Float_Array.ID,
                                    Stride = 16,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Type = "float4x4",
                                        },
                                    },
                                },
                            },
                        };
                    }
                    #endregion

                    var c = new Grendgine_Collada_Controller()
                    {
                        ID = "Controller_" + MakeID(t),
                        Skin = new Grendgine_Collada_Skin()
                        {
                            SourceAt = "#" + geometriesDic[t].ID,
                            Source = new Grendgine_Collada_Source[]
                            {
                                Joints_Source,
                                Weights_Source,
                                Inv_Bind_Mats_Source,
                            },
                            Joints = new Grendgine_Collada_Joints()
                            {
                                Input = new Grendgine_Collada_Input_Unshared[]
                                {
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.JOINT,
                                        source = "#" + Joints_Source.ID,
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.INV_BIND_MATRIX,
                                        source = "#" + Inv_Bind_Mats_Source.ID,
                                    },
                                },
                            },
                            Vertex_Weights = new Grendgine_Collada_Vertex_Weights()
                            {
                                Count = (uint)boneWeights.Length,
                                Input = new Grendgine_Collada_Input_Shared[]
                                {
                                    new Grendgine_Collada_Input_Shared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.JOINT,
                                        source = "#" + Joints_Source.ID,
                                        Offset = 0,
                                        Set = 0,
                                    },
                                    new Grendgine_Collada_Input_Shared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.WEIGHT,
                                        source = "#" + Weights_Source.ID,
                                        Offset = 1,
                                        Set = 0,
                                    },
                                },
                                VCount = new Grendgine_Collada_Int_Array_String()
                                {
                                    Value_As_String = weightsVCountString.ToString(),
                                },
                                V = new Grendgine_Collada_Int_Array_String()
                                {
                                    Value_As_String = weightsVString.ToString(),
                                },
                            },
                        },
                    };
                    controllersDic.Add(t, c);
                    #region Node
                    nodesDic[t].Instance_Controller = new Grendgine_Collada_Instance_Controller[]
                    {
                        new Grendgine_Collada_Instance_Controller()
                        {
                            URL = "#" + c.ID,
                            Bind_Material = nodesDic[t].Instance_Geometry[0].Bind_Material,
                        },
                    };
                    nodesDic[t].Instance_Geometry = null;
                    nodesDic[t].Name = "Mesh";
                    #endregion
                }
                lc.Controller = controllersDic.Values.ToArray();
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Controllers", (progressIndex++ / (float)progressTotal));

            #region Etc
            #region RemoveBlankNode
            {
                Func<Grendgine_Collada_Node, bool> CheckNotBlank = null;
                CheckNotBlank = (n) =>
                {
                    if (n.node != null && n.node.Length > 0)
                    {
                        for (int i = 0; i < n.node.Length; i++)
                        {
                            if (CheckNotBlank(n.node[i]))
                                return true;
                        }
                        n.node = null;
                    }
                    return (n.Instance_Geometry != null && n.Instance_Geometry.Length > 0) ||
                           (n.Instance_Controller != null && n.Instance_Controller.Length > 0);
                };
                var node = nodesDic[transforms[0]];
                if (!CheckNotBlank(node))
                {
                    node.node = null;
                }
            }
            #endregion
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Etc", (progressIndex++ / (float)progressTotal));

            #region Scene
            {
                gCollada.Library_Visual_Scene = new Grendgine_Collada_Library_Visual_Scenes()
                {
                    Visual_Scene = new Grendgine_Collada_Visual_Scene[]
                    {
                        new Grendgine_Collada_Visual_Scene()
                        {
                            ID = "Scene_" + MakeID(transforms[0].gameObject),
                            Name = "Scene",
                            Node =new Grendgine_Collada_Node[]
                            {
                                nodesDic[transforms[0]],
                            },
                        },
                    },
                };
                if (jointsDic.Count > 0)
                {
                    ArrayUtility.Insert(ref gCollada.Library_Visual_Scene.Visual_Scene[0].Node, 0, jointsDic[transforms[0]]);
                }
                gCollada.Scene = new Grendgine_Collada_Scene()
                {
                    Visual_Scene = new Grendgine_Collada_Instance_Visual_Scene()
                    {
                        URL = "#" + gCollada.Library_Visual_Scene.Visual_Scene[0].ID,
                    },
                };
            }
            #endregion
            EditorUtility.DisplayProgressBar(progressTitle, "Scene", (progressIndex++ / (float)progressTotal));

            #region Write
            using (var writer = new StreamWriter(path))
            {
                var xmlSerializer = new XmlSerializer(typeof(Grendgine_Collada));
                xmlSerializer.Serialize(writer, gCollada);
            }
            exportedFiles.Add(path);
            #endregion
            EditorUtility.ClearProgressBar();

            return true;
        }

        public List<string> exportedFiles = new List<string>();
    }
}
