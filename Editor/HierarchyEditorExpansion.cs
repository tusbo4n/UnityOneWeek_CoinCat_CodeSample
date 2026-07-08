using UnityEditor;
using UnityEngine;

public class HierarchyEditorExpansion : MonoBehaviour
{
    [MenuItem("GameObject/CreateEmptyBeside(Shift+S) #s", false, 0)]
    static void CreateEmptyBesideSelected()
    {
        // 選択中のオブジェクトを取得
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("何も選択されていません。空のゲームオブジェクトを作成するには、ヒエラルキーでオブジェクトを選択してください。");
            return;
        }

        // アンドゥの登録開始
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        // 新しい空のゲームオブジェクトを作成
        GameObject newObject = new GameObject("EmptyObject");
        Undo.RegisterCreatedObjectUndo(newObject, "Create Empty Object Beside");

        // 同じ親を設定
        newObject.transform.parent = selectedObject.transform.parent;

        // 同じ座標に配置
        newObject.transform.position = selectedObject.transform.position;

        // 選択中のオブジェクトの「すぐ上」に配置
        int siblingIndex = selectedObject.transform.GetSiblingIndex();
        Undo.SetSiblingIndex(newObject.transform, siblingIndex, "Set Sibling Index");

        // アンドゥグループを確定
        Undo.CollapseUndoOperations(undoGroup);

        // 新しいオブジェクトを選択
        Selection.activeGameObject = newObject;
    }
}
