using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Security.Cryptography;

namespace subMsg
{
 	public partial class Form1 : Form
	{
		Dictionary<string, string> config_dict = new Dictionary<string, string>();
		
		[STAThread]
		static void Main()
		{
			string[] args = Environment.GetCommandLineArgs();
			Form1 f = new Form1();
			String fileToOperate = "NONE";
			String fileToSave = "NONE";
			String key = "NONE";
			try
			{
				fileToOperate = args[1];
				fileToSave = args[2];
				key = args[3];
			} catch (Exception e)
			{
				Console.WriteLine("<prog> <file to operate on> <file to save> <key>");
				return;
			}
			try
			{
		    		string msgs = File.ReadAllText(fileToOperate);
		    		string decrypted = f.Decrypt(msgs, key);
				File.WriteAllText(fileToSave, decrypted);
			}
			catch
			{
				Console.WriteLine("{0} or {1} not present", fileToOperate, fileToSave);
			}
		}
		
		/*
		 * Encryption
		 *
		 */
		private string Encrypt(string clearText, string encryptionKey)
		{
			byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

			using (Aes encryptor = Aes.Create())
			{
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cs.Write(clearBytes, 0, clearBytes.Length);
						cs.Close();
					}
					clearText = Convert.ToBase64String(ms.ToArray());
				}
			}
			return clearText;
		}

		/*
		 * Decryption
		 *
		 */
		 private string Decrypt(string cipherText, string encryptionKey)
		 {
			byte[] cipherBytes = Convert.FromBase64String(cipherText);
			using (Aes encryptor = Aes.Create())
			{
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(cipherBytes, 0, cipherBytes.Length);
						cs.Close();

					}
					cipherText = Encoding.Unicode.GetString(ms.ToArray());
				}
			}
			return cipherText;
		 }
				 

	}
}