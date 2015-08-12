/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.08.2015
 * Time: 10:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;

namespace Telemonitor
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{	
		private TelegramWorker worker;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}		
				
		
		void MainFormShown(object sender, EventArgs e)
		{						
			Hide();
		}
		
				
		void MainFormLoad(object sender, EventArgs e)
		{						
						
			Settings tmSettings = new Settings();
			
			if (tmSettings.SettingsExists) {				
				worker = new TelegramWorker(tmSettings);
				worker.StartWork();
				trayIcon.Text = "Telemonitor (работает)";
			}
			else
			{
				trayIcon.Text = "Telemonitor (неактивен)";
			}
		}
			
				
		
		void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (worker != null)
				worker.StopWork();
		}
	}
}
