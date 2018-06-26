using System.IO;
using System.Text;
using Icarus.GameFramework;
using UnityEditor;
using UnityEngine;

namespace Icarus.UnityGameFramework.Editor
{
    public class StreamingAssetsVersionCreate
    {
        [MenuItem("Icarus/Game Framework/AssetBundle Tools/StreamingAssets Version 生成")]
        static void Create()
        {
            var filePaths = Directory.GetFiles(Application.streamingAssetsPath, "*~version.dat",
                SearchOption.AllDirectories);

            if (filePaths.Length == 0)
            {
                throw new GameFrameworkException("在"+ Application.streamingAssetsPath + "目录下,没有找到ab包的version文件");
            }

            string AbListPath =
                Icarus.GameFramework.Utility.Path.GetCombinePath(Application.streamingAssetsPath, "version");

            if (File.Exists(AbListPath))
            {
                File.Delete(AbListPath);
            }

            
            StringBuilder sb = new StringBuilder();

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                fileName = fileName.Split('.')[0];
                Debug.Log(fileName);
                sb.Append(fileName + "|");

            }
            sb.Remove(sb.Length - 1, 1);
            byte[] encryptBytes = new byte[4];
            Icarus.GameFramework.Utility.Random.GetRandomBytes(encryptBytes);
            using (FileStream fileStream = new FileStream(AbListPath, FileMode.CreateNew, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(encryptBytes);
                    Debug.Log(sb);
                    var by = GetXorBytes(Icarus.GameFramework.Utility.Converter.GetBytes(sb.ToString()), encryptBytes);
                    binaryWriter.Write(by);
                }
            }

            File.Move(AbListPath, Icarus.GameFramework.Utility.Path.GetResourceNameWithSuffix(AbListPath));

        }
        
        private static byte[] GetXorBytes(byte[] bytes, byte[] code, int length = 0)
        {
            if (bytes == null)
            {
                return null;
            }

            int codeLength = code.Length;
            if (code == null || codeLength <= 0)
            {
                throw new GameFrameworkException("Code is invalid.");
            }

            int codeIndex = 0;
            int bytesLength = bytes.Length;
            if (length <= 0 || length > bytesLength)
            {
                length = bytesLength;
            }

            byte[] result = new byte[bytesLength];
            System.Buffer.BlockCopy(bytes, 0, result, 0, bytesLength);

            for (int i = 0; i < length; i++)
            {
                result[i] ^= code[codeIndex++];
                codeIndex = codeIndex % codeLength;
            }

            return result;
        }
    }
}