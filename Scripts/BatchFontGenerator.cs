using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BatchFontGenerator : ScriptableObject
{
    const string RANGE_KR = "32-126,44032-55203,12593-12643,8200-9900";


    [MenuItem("Tools/Generate TMP FontAssets from Selected-Korean")]
    public static void GenerateTMPFontAssetsKR()
    {
        GenerateTMPFontAssets(RANGE_KR);
    }


    public static void GenerateTMPFontAssets(string characterSequence)
    {
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("변환할 폰트 파일을 선택해주세요.");
            return;
        }

        int successCount = 0;
        uint[] characters = ParseSequence(characterSequence);

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            if (assetPath.EndsWith(".ttf") || assetPath.EndsWith(".otf"))
            {
                Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(assetPath);
                if (sourceFont != null)
                {
                    string directory = Path.GetDirectoryName(assetPath);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    string savePath = Path.Combine(directory, fileName + " SDF.asset");

                    TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
                    fontAsset.TryAddCharacters(characters);
                    AssetDatabase.CreateAsset(fontAsset, savePath);

                    if (fontAsset.material != null)
                    {
                        fontAsset.material.name = fileName + " Material";
                        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                    }
                    if (fontAsset.atlasTexture != null)
                    {
                        fontAsset.atlasTexture.name = fileName + " Atlas";
                        AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
                    }

                    EditorUtility.SetDirty(fontAsset);
                    successCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{successCount}개의 TMP_FontAsset 생성이 완료되었습니다.");
    }

    private static uint[] ParseSequence(string sequence)
    {
        List<uint> list = new List<uint>();
        foreach (var part in sequence.Split(','))
        {
            var range = part.Split('-');
            if (range.Length == 1 && uint.TryParse(range[0].Trim(), out uint c))
                list.Add(c);
            else if (range.Length == 2 && uint.TryParse(range[0].Trim(), out uint start) && uint.TryParse(range[1].Trim(), out uint end))
                for (uint i = start; i <= end; i++)
                    list.Add(i);
        }
        return list.Distinct().ToArray();
    }
}
