using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

namespace Studio.Framework
{
    public class CsvRow {
        private List<string> data = null;
        private int index = 0;
        public int Count => data?.Count ?? 0;

        public CsvRow(List<string> data) {
            this.data = data;
        }
        
        public void Init() {
            index = 0;
        }
        
        public bool IsEmpty() {
            foreach (var col in data) {
                if (!string.IsNullOrEmpty(col.Trim())) {
                    return false;
                }
            }
            return true;
        }
        
        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < data.Count; i++) {
                var column = data[i];
                builder.Append(column);
                builder.Append(i==(data.Count-1) ? "" : ",");
            }
            
            return builder.ToString();
        }

        // random access apis
        //-------------------------------------------------------------------------
        public string AsString(int column) {
            return data[column];
        }

        public int AsInt(int column) {
            return Convert.ToInt32(data[column]);
        }

        public float AsFloat(int column) {
            return Convert.ToSingle(data[column]);
        }

        public bool AsBool(int column) {
            return data[column] == "TRUE";
        }

        public T AsEnum<T>(int column) where T : struct {
            return (T)Enum.Parse(typeof(T), AsString(column), true);
        }


        // next series apis
        //-------------------------------------------------------------------------
        public string NextString() {
            return data[index++];
        }

        public int NextInt() {
            return Convert.ToInt32(data[index++]);
        }

        public float NextFloat() {
            return Convert.ToSingle(data[index++]);
        }

        public bool NextBool() {
            return data[index++] == "TRUE";
        }

        public T NextEnum<T>() where T : struct {
            return (T)Enum.Parse(typeof(T), NextString(), true);
        }
    }

    public class CsvParser : System.Collections.Generic.IEnumerable<CsvRow>
    {
        private char[] delimiters = new char[] {'\r', '\n'};
        private List<CsvRow> data = new List<CsvRow>();

        public CsvParser() {}
        
        public CsvParser(string text)
        {
            Parse(text);
        }

        public void Parse(string text, char delimiter = ',') {
            string[] lines = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            data.Clear();
            foreach (string line in lines) {
                if (string.IsNullOrEmpty(line) == true) {
                    continue;
                }

                if (line[0] == '#') {
                    continue;
                }

                string[] token = line.Split(delimiter);
                List<string> parts = new List<string>();

                foreach(string word in token) {
                    parts.Add(word);
                }

                data.Add(new CsvRow(parts));
            }
        }

        public int Count {
            get { return data.Count; }
        }

        public CsvRow GetRow(int index) {
            return data[index];
        }
        
        public CsvRow GetRowColumnInit(int index) {
            data[index].Init();
            return data[index];
        }

        public IEnumerator<CsvRow> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }
}