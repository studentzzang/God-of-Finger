// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;
//
// /// <summary>
// /// 선택한 JSON(TextAsset)을 DialogueSO(.asset)로 변환/저장한다.
// /// - Create: 같은 폴더에 .asset 생성
// /// - Update: 동일 이름의 .asset이 있으면 덮어쓰기 갱신
// /// </summary>
// public static class DialogueJsonToSoImporter
// {
//     [MenuItem("Tools/Dialogue/Convert Selected JSON To DialogueSO (Create or Update)")]
//     public static void ConvertSelectedJson()
//     {
//         var json = Selection.activeObject as TextAsset;
//         if (json == null)
//         {
//             EditorUtility.DisplayDialog("Dialogue Import", "Project 창에서 JSON(TextAsset) 파일을 선택하세요.", "OK");
//             return;
//         }
//
//         // 1) JSON -> DialogueSO (메모리 인스턴스)
//         var parsed = DialogueJsonParser.ParseFromJson(json);
//         if (parsed == null)
//         {
//             EditorUtility.DisplayDialog("Dialogue Import", "파싱 실패. JSON 형식을 확인하세요.", "OK");
//             return;
//         }
//
//         // 2) 저장 경로(기본: JSON과 같은 폴더, 같은 이름)
//         string jsonPath = AssetDatabase.GetAssetPath(json);
//         string folder = System.IO.Path.GetDirectoryName(jsonPath);
//         string baseName = System.IO.Path.GetFileNameWithoutExtension(jsonPath);
//         string assetPath = $"{folder}/{baseName}.asset";
//
//         // 3) 이미 동일 경로에 에셋이 있으면 Update, 없으면 Create
//         var existing = AssetDatabase.LoadAssetAtPath<DialogueSO>(assetPath);
//
//         if (existing == null)
//         {
//             // Create (경로 충돌 방지)
//             assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
//             AssetDatabase.CreateAsset(parsed, assetPath);
//         }
//         else
//         {
//             // Update: 기존 에셋에 parsed 값을 복사 후 parsed는 폐기
//             CopyInto(existing, parsed);
//             Object.DestroyImmediate(parsed);
//             EditorUtility.SetDirty(existing);
//         }
//
//         AssetDatabase.SaveAssets();
//         AssetDatabase.Refresh();
//
//         var finalAsset = AssetDatabase.LoadAssetAtPath<DialogueSO>(assetPath);
//         Selection.activeObject = finalAsset;
//
//         EditorUtility.DisplayDialog("Dialogue Import", $"완료!\n{assetPath}", "OK");
//     }
//
//     private static void CopyInto(DialogueSO dst, DialogueSO src)
//     {
//         // ScriptableObject는 통째로 교체가 아니라 필드 복사가 안전함
//         dst.lines = src.lines;
//
//         dst.hasChoice = src.hasChoice;
//         dst.choicePromptLine = src.choicePromptLine;
//         dst.acceptResult = src.acceptResult;
//         dst.rejectResult = src.rejectResult;
//
//         dst.acceptLabel = src.acceptLabel;
//         dst.rejectLabel = src.rejectLabel;
//     }
// }
// #endif