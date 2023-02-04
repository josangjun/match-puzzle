using GoogleSheetsToUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Game
{
    public class GoogleSheetBridge : MonoBehaviour
    {
        public string sheetId = "11eHK5Qcqx1b3L-k9MkkuAdfaLMvNhXfz-MINSL_eEmA";
        public string sheetName = "sheet1";
        
        public static bool isDone { get; private set; }

        public static Coroutine UpdateGSTU(GameObject go)
        {
            var bridge = go.GetComponent<GoogleSheetBridge>();
            if (bridge != null)
            {
                return bridge.UpdateGSTU();
            }
            return null;
        }

        private Coroutine UpdateGSTU()
        {
            return StartCoroutine(UpdateGSTU_());
        }

        private IEnumerator UpdateGSTU_()
        {
            isDone = false;
            //https://docs.google.com/spreadsheets/d/11eHK5Qcqx1b3L-k9MkkuAdfaLMvNhXfz-MINSL_eEmA/edit#gid=0
            SpreadsheetManager.ReadPublicSpreadsheet(new GSTU_Search(
                sheetId, sheetName), sheet =>
                {
                    try
                    {
                        ReadFromWorksheet(sheet);
                    }
                    catch (System.Exception e)
                    {
                        SLogger.LogError(e.Message + "\n" + e.StackTrace);
                    }
                    finally
                    {
                        isDone = true;
                    }
                });

            var elapsed = 0f;
            while (!isDone)
            {
                yield return null;
                elapsed += Time.deltaTime;
                if (elapsed > 10f)
                {
                    isDone = true;
                    Debug.LogWarning($"GSTU timeout:{elapsed}");
                    break;
                }
            }
        }

        const string kValue = "Value";
        private readonly System.Type[] types = new[] { typeof(string) }; 
        private readonly object[] args = new object[1];

        private void ReadFromWorksheet(GstuSpreadSheet sheet)
        {
            var config = Stage.config;

            var bindings = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;

            foreach (var key in sheet.columns["A"])
            {
                try
                {
                    var val = sheet[key.value, kValue];
                    
                    var field = config.GetType().GetField(key.value);
                    if (field == null)
                        continue;

                    if (field.FieldType.IsPrimitive)
                    {
                        args[0] = val.value;
                        var ret = field.FieldType.GetMethod("Parse", bindings, null, types, null).Invoke(null, args);
                        field.SetValue(config, ret);
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(config, val.value);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}