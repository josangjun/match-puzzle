using System;
using System.Text;

namespace Opencoding.Console
{
	public class SaveFileData
	{
		public byte[] Data { get; set; }

		public string Name { get; set; }

		public SaveFileDataType DataType { get; set; }

		public SaveFileData(string name, byte[] data, SaveFileDataType dataType)
		{
			Name = name;
			Data = data;
			DataType = dataType;
		}

		public SaveFileData(string name, string data, SaveFileDataType dataType)
		{
			Name = name;
			Data = Encoding.UTF8.GetBytes(data);
			DataType = dataType;
		}

		public string GetMimeType()
		{
			return DataType switch
			{
				SaveFileDataType.TEXT => "text/plain", 
				SaveFileDataType.BINARY => "application/octet-stream", 
				SaveFileDataType.JSON => "application/json", 
				_ => throw new InvalidOperationException("Unrecognised save file data type: " + DataType), 
			};
		}
	}
}
