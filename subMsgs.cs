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
    	/*
     	* Create a class which is a Child of Form
     	* 
     	*/
 	public partial class Form1 : Form
	{
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		enum KeyModifier
		{
		    None = 0,
		    Alt = 1,
		    Control = 2,
		    Shift = 4,
		    WinKey = 8
		}

		private Button quitButton;
		private Label label2;

		private Button loadMessagesButton;
		private Button clearWindowButton;
		private Button pauseButton;

		/*
		 * Messages and configuration file contents as lists
		 *
		 */
		string[] messages;
		string[] config;

		/*
		 * Dictionaries for configuration file contents, return values and colors
		 *
		 */
		Dictionary<string, string> config_dict = new Dictionary<string, string>();
		Dictionary<int, string> rvalues_dict = new Dictionary<int, string>();
		Dictionary<string, int> msg_display_types = new Dictionary<string, int>();

		/*
		 * Timer related variables
		 * 
		 */
		int init_timer_sleep = 2000;
		int timer_sleep = 2000;
		int timer_change_rate = 100;
		int shown = 0;
		// int messages_loaded = 0;
		// int debug = 0;


		/*
		 * Global variables
		 * 
		 */
		int config_correct = 0;
		int delay_time = 2000;
		int blink_time = 1000;
		int is_bold = 0;

		int msg_display_speed = 0;
		float font_size = 30;
		string font_type = "Arial";
		string font_color = "Red";
		Random xpos = new Random();
		Random ypos = new Random();


		/*
		 * Screen resolution
		 * 
		 */
		int screen_width = SystemInformation.VirtualScreen.Width;
		int screen_height = SystemInformation.VirtualScreen.Height;

		int num_config_items_read = 0;

		/*
		 * The label which will hold the text to be displayed, and flashed
		 * 
		 */
		Label label1 = new Label();

		int rvalue = 0;

		/*
		 * Initialize two timers - 
		 *      timer1 - delay between flashes
		 *      timer2 - flashing time
		 *      
		 */
		System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
		System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();

		public int pause_msgs;
		public int Pause_Msgs
		{
		    	get { return pause_msgs; }
		    	set
		    	{
				pause_msgs = value;
		    	}
		}

		/*
		 * This function is run whenever a message is sent to a window
		 *
		 */
		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == 0x0312)
			{
				/*
				 * Note that the three lines below are not needed if you only want to register one hotkey.
				 * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument, or if you want to know which key/modifier was pressed for some particular reason
				 *
				 */

				Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
				KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
				int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.

				// MessageBox.Show(key.ToString());
				/*
				 * Below sleep is important for the hotkeys to function
				 *
				 */
				if (key.ToString() == "E")
				{
					if (pause_msgs == 0)
					{
						pauseButton.Text = "Resume";
						Pause_Msgs += 1;
					}
					else
					{
						pauseButton.Text = "Pause";
						Pause_Msgs -= 1;
					}
				}
				if (key.ToString() == "D")
				{
					if (this.shown == 0)
					{
						quitButton.Show();
						loadMessagesButton.Show();
						clearWindowButton.Show();
						pauseButton.Show();
						this.shown = 1;
					}
					else
					{
						quitButton.Hide();
						loadMessagesButton.Hide();
						clearWindowButton.Hide();
						pauseButton.Hide();
						this.shown = 0;
					}					
				}
	    		}
		}


		public Form1()
		{

			/*
			 * Return values mapped to errors/messages
			 * 
			 */
			rvalues_dict[3] = "Unable to set Label parameters and Global variables";
			rvalues_dict[1] = "Unable to set Global variables";
			rvalues_dict[2] = "Unable to set Label parameters";
			rvalues_dict[20] = "Invalid Configuration File";
			rvalues_dict[30] = "Invalid Messages File";

			/*
			 * How messages are displayed?
			 * 
			 */
			msg_display_types["normal"] = 0;
			msg_display_types["slowing"] = 1;
			msg_display_types["faster"] = 2;
			msg_display_types["sync_with_music"] = 3;

			/*
			 * Read and process configuration file
			 * 
			 */
			rvalue = readAndProcessConfig();

			if (rvalue != 0)
			{
				MessageBox.Show(rvalues_dict[rvalue]);
				this.Close();
				Application.Exit();
			}

			/*
			 * Initialize component
			 * 
			 */
			InitializeComponent();

			/*
			 * Form background, border and transparency
			 * 
			 */
			this.BackColor = Color.White;
			this.TransparencyKey = Color.White;
			this.FormBorderStyle = FormBorderStyle.None;
			this.Bounds = Screen.PrimaryScreen.Bounds;
			this.TopMost = true;
			this.Name = "subMsgs";
			this.Text = "subMsgs";
			this.Size = new Size(screen_width, screen_height);
			label2.Width = 1000;
			label2.Height = 1000;
			label1.Width = 1000;
			label1.Height = 40;
			label1.AutoSize = false;

			/*
			 * button1 - quit
			 * button2 - Load Messages
			 * 
			 */
			this.quitButton.Click += new EventHandler(quitButton_Click);
			this.loadMessagesButton.Click += new EventHandler(loadMessagesButton_Click);
			this.clearWindowButton.Click += new EventHandler(clearWindowButton_Click);
			this.pauseButton.Click += new EventHandler(pauseButton_Click);

			/*
			 * Add label1 to form controls
			 * 
			 */
			this.Controls.Add(label1);

			/*
			 * Show form
			 * 
			 */
			this.Show();

			int id = 0;     // The id of the hotkey. 
			RegisterHotKey(this.Handle, id, (int)KeyModifier.Shift | (int)KeyModifier.Alt, Keys.E.GetHashCode());
			RegisterHotKey(this.Handle, id, (int)KeyModifier.Shift | (int)KeyModifier.Alt, Keys.D.GetHashCode());
			
			/*
			 * Display flashing text
			 *
			 */
			blinkLabel();
		}

		/*
		 * Some event handlers
		 * 
		 */
		private void quitButton_Click(object sender, EventArgs e)
		{
			this.Close();
		    	Application.Exit();
		}

		public void pauseButton_Click(object sender, EventArgs e)
		{
		    	if (pause_msgs == 0)
		    	{
				pauseButton.Text = "Resume";
				Pause_Msgs += 1;
		    	}
		    	else
		    	{
				pauseButton.Text = "Pause";
				Pause_Msgs -= 1;
		    	}
		}

		private void loadMessagesButton_Click(object sender, EventArgs e)
		{
		    	string[] temp = readMsgsFile();
		    	if (temp.Length != 0)
		    	{
				messages = temp;
		    	}
		    	else
		    	{
				MessageBox.Show(rvalues_dict[30]);
		    	}
		}

		private void clearWindowButton_Click(object sender, EventArgs e)
		{
		    	quitButton.Hide();
		    	loadMessagesButton.Hide();
		    	clearWindowButton.Hide();
		    	pauseButton.Hide();
		}

		/*
		 * Action: Read configuration file
		 * Returns: Array of lines in configuration file
		 *
		 */
		string[] readConfigFile()
		{
		    	Stream myStream = null;
		    	string[] lines = new string[] { };

		    	OpenFileDialog openFileDialog1 = new OpenFileDialog();

		    	openFileDialog1.Filter = "subMsg Configuration Files (.conf)|*.conf|All Files (*.*)|*.*";
		    	openFileDialog1.FilterIndex = 1;
		    	openFileDialog1.Multiselect = false;
	
		    	if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		    	{
				if ((myStream = openFileDialog1.OpenFile()) != null)
				{
				    string strfilename = openFileDialog1.FileName;
				    lines = File.ReadAllLines(strfilename);
				    myStream.Close();
				}
		    	}
		    	return lines;
		}

		/*
		 * Action: Read messages file
		 * Returns: Array of messages
		 * 
		 */
		string[] readMsgsFile()
		{
		    	Stream myStream = null;
		    	string[] lines = new string[] { };

		    	OpenFileDialog openFileDialog1 = new OpenFileDialog();
	
		    	openFileDialog1.Filter = "subMsg Message Files (.smsg)|*.smsg|All Files (*.*)|*.*";
		    	openFileDialog1.FilterIndex = 1;
		    	openFileDialog1.Multiselect = false;
	
		    	if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		    	{
				if ((myStream = openFileDialog1.OpenFile()) != null)
				{
				    	string strfilename = openFileDialog1.FileName;
				    	string content = File.ReadAllText(strfilename);
					string decrypted = Decrypt(content);
					lines = decrypted.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
					myStream.Close();
				}
		    	}
		    	return lines;
		}	

        	/*
        	 * Encryption
        	 *
        	 */
		private string Encrypt(string clearText)
		{
			string encryptionKey = config_dict["key"];
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
			MessageBox.Show(clearText);
			return clearText;
		}
        	
        	/*
        	 * Decryption
        	 *
        	 */
		 private string Decrypt(string cipherText)
		 {
		     	string encryptionKey = config_dict["key"];
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
		 
 
		/*
		 * Action: Start the timers to display the messages
		 * 
		 */
		void timerFunc(string type_of_timer)
		{
			int msg_index = 0;

			//
			// Below code is only for debug purposes:
			//
			// int display_times = 0;
			//

			timer1.Tick += new System.EventHandler((s1, e1) =>
			{
				try
				{
				    	if ((pause_msgs == 0) && (messages.Length > 0) && (config_correct == 0))
				    	{
						Point location = new Point(xpos.Next(1, (int) (screen_width * (3.0/4.0))), ypos.Next(1, screen_height));
						label1.Location = location;

						msg_index = msg_index + 1;

						if (msg_index >= messages.Length)
						{
					    		msg_index = 0;
						}

						label1.Text = messages[msg_index];

				       	//
				       	// Below code is just for debug purposes.
				       	// display_times++;
				       	// if (display_times > 10000)
				       	// {
				       	//     display_times = 0;
				       	// }
				       	// 
				       	// if (debug == 1)
				       	// {
				       	//     label2.Text = display_times.ToString() + ", " + location.ToString() + "\n";
				       	//     label2.Text += messages[msg_index] + "\n";
				       	//     label2.Text += label1.Visible.ToString();
				       	// }
				       	//
	
					label1.Visible = true;
					label1.Refresh();

					timer2.Tick += new System.EventHandler((s2, e2) =>
					{
					    	label1.Visible = false;
					    	label1.Refresh();
					    	timer2.Stop();
					});
					timer2.Start();
				}
				else
				{
					// timer1.Stop();
				}
			}
			catch
			{

			}

			if (type_of_timer == "faster")
			{
				timer_sleep = timer_sleep - timer_change_rate;
				if (timer_sleep <= 0)
				{
					timer_sleep = init_timer_sleep;
				}
				timer1.Interval = timer_sleep;
				} else if (type_of_timer == "slowing")
				{
				    	timer_sleep = timer_sleep + timer_change_rate;
				    	if (timer_sleep >= 10000)
				    	{
						timer_sleep = init_timer_sleep;
				    	}
				    	timer1.Interval = timer_sleep;
				}
			});
			timer1.Start();
		}


		/*
		 * Action: Flash messages on the screen with two parameters:
		 *          1. Flashing time (time for which message is displayed on screen during flash)
		 *          2. Flashing delay (time delay between flashing of messages)
		 *
		 */
		void blinkLabel()
		{
			timer1.Interval = delay_time;
			timer2.Interval = blink_time;

			// int msg_index = 0;

			if (msg_display_speed == msg_display_types["normal"]) {
				timerFunc("normal");        
			}
			else if (msg_display_speed == msg_display_types["faster"]) {
				timerFunc("faster");
			}
			else if (msg_display_speed == msg_display_types["slowing"])
			{
				timerFunc("slowing");
			}

		}

		/*
		 * Set the properties read from configuration file, to the following:
		 *      1. Global variables
		 *      2. Label properties
		 * Returns: Result of action - 0 for success, non-zero for failure
		 *
		 */
		int setProps()
		{
			int rvalue = 0;
			int result = 0;

			rvalue = setGlobalVars();
			result += rvalue;
			rvalue = setLabelProps();
			result += rvalue;

			return result;
		}

		/*
		 * Action: Set global variables
		 * Returns: Result of action - 0 for success, non-zero for failure
		 * 
		 */
		int setGlobalVars()
		{
			try
			{
				delay_time = Int32.Parse(config_dict["delay_time"]);
				blink_time = Int32.Parse(config_dict["blink_time"]);
				msg_display_speed = msg_display_types[config_dict["msg_display_speed"]];
				timer_change_rate = Int32.Parse(config_dict["timer_change_rate"]);
				timer_sleep = Int32.Parse(config_dict["timer_sleep"]);
				init_timer_sleep = timer_sleep;

				timer1.Interval = delay_time;
				timer2.Interval = blink_time;
			}
			catch
			{
				return 2;
			}
			return 0;
		}

		/*
		 * Action: Set label properties
		 * Returns: Result of action - 0 for success, non-zero for failure
		 * 
		 */
		int setLabelProps()
		{
			try
			{
				// printConfig();

				font_type = config_dict["font_type"];

				font_size = (float)Int32.Parse(config_dict["font_size"]);

				is_bold = Int32.Parse(config_dict["is_bold"]);


				FontFamily family = new FontFamily(font_type);
				if (is_bold == 1)
				{
					label1.Font = new Font(family, font_size, FontStyle.Bold);
				} else
				{
					label1.Font = new Font(family, font_size, FontStyle.Regular);
				}
				font_color = config_dict["font_color"];
				label1.ForeColor = Color.FromName(font_color);

				label1.TextAlign = ContentAlignment.TopLeft;
				label1.Location = new Point(100, 100);
				label1.Visible = false;
				label1.Refresh();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return 1;
			}
			return 0;
		}

		/*
		 * Action: Fill the config_hash with contents received from configuration file, into config array
		 * 
		 */
		private void processConfig()
		{
		    	foreach (string line in config)
		    	{
				string[] tokens = line.Split('=');
				string left = "NO_LINE";
				string right = "NO_VALUE";

				try
				{
			    	left = tokens[0].Trim();
			    	right = tokens[1].Trim();
				}
				catch
				{
			    	config_correct = 1;
				}
				if (!config_dict.ContainsKey(left))
				{
					config_dict.Add(left, right);
				}
				else
				{
			    		config_dict[left] = right;
				}
				
				if (num_config_items_read == 4)
				{
			    		config_correct = 1;
			    		num_config_items_read = 0;
				}

		    	}
		}

		/*
		 * Action: Display configuration
		 * 
		 */
		private void printConfig()
		{
		    	string content = "";
		    	foreach (KeyValuePair<string, string> entry in config_dict)
		    	{
				content += entry.Key;
				content += "->";
				content += entry.Value;
				content += ".\n";
		    	}
		    	MessageBox.Show(content);

		}

		/*
		 * Action: Read configuration file, and update the values read from configuration file, onto the configuration hash
		 * Returns: Result of action - 0 for success, non-zero for failure
		 * 
		 */
		private int readAndProcessConfig()
		{
		    	string[] temp = readConfigFile();
		    	int rvalue = 0;
		    	if (temp.Length != 0)
		    	{
				config = temp;
		    	}
		    	else
		    	{
				rvalue = 20;
				return rvalue;
		    	}
		    	processConfig();
	
		    	rvalue = setProps();
		    	return rvalue;
		}

		protected override void OnKeyPress(KeyPressEventArgs ex)
		{
			string xo = ex.KeyChar.ToString();

			if (xo == "l")
			{
			    	quitButton.Show();
			    	loadMessagesButton.Show();
			    	clearWindowButton.Show();
			    	pauseButton.Show();
		    	}
		}
		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.quitButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.loadMessagesButton = new System.Windows.Forms.Button();
			this.clearWindowButton = new System.Windows.Forms.Button();
			this.pauseButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// quitButton
			// 
			this.quitButton.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.quitButton.Location = new System.Drawing.Point(276, 309);
			this.quitButton.Name = "quitButton";
			this.quitButton.Size = new System.Drawing.Size(400, 100);
			this.quitButton.TabIndex = 0;
			this.quitButton.Text = "Quit";
			this.quitButton.UseVisualStyleBackColor = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1, 247);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(0, 13);
			this.label2.TabIndex = 3;
			// 
			// loadMessagesButton
			// 
			this.loadMessagesButton.Location = new System.Drawing.Point(12, 12);
			this.loadMessagesButton.Name = "loadMessagesButton";
			this.loadMessagesButton.Size = new System.Drawing.Size(400, 100);
			this.loadMessagesButton.TabIndex = 4;
			this.loadMessagesButton.Text = "Load Messages";
			this.loadMessagesButton.UseVisualStyleBackColor = true;
			// 
			// clearWindowButton
			// 
			this.clearWindowButton.Location = new System.Drawing.Point(12, 153);
			this.clearWindowButton.Name = "clearWindowButton";
			this.clearWindowButton.Size = new System.Drawing.Size(400, 100);
			this.clearWindowButton.TabIndex = 5;
			this.clearWindowButton.Text = "Clear Window";
			this.clearWindowButton.UseVisualStyleBackColor = true;
			// 
			// pauseButton
			// 
			this.pauseButton.Location = new System.Drawing.Point(537, 12);
			this.pauseButton.Name = "pauseButton";
			this.pauseButton.Size = new System.Drawing.Size(397, 100);
			this.pauseButton.TabIndex = 6;
			this.pauseButton.Text = "Pause";
			this.pauseButton.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(987, 421);
			this.Controls.Add(this.pauseButton);
			this.Controls.Add(this.clearWindowButton);
			this.Controls.Add(this.loadMessagesButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.quitButton);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}	
		
		#endregion
	
		/*
		 * Unregister HotKey when form closes
		 *
		 */
        	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        	{
        	    	UnregisterHotKey(this.Handle, 0);       // Unregister hotkey with id 0 before closing the form. You might want to call this more than once with different id values if you are planning to register more than one hotkey.
        	}
       	
		[STAThread]
		static void Main()
		{
        		Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
        	    	Application.Run(new Form1());
		}
    	}	
}