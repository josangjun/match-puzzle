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

        private void ReadFromWorksheet(GstuSpreadSheet sheet)
        {
            var config = Stage.Instance.config;

            config.fallSpeed = float.Parse(sheet["fallSpeed", kValue].value);
            config.gemTypeCount = int.Parse(sheet["gemTypeCount", kValue].value);
        }
    }
}