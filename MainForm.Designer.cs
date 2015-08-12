/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.08.2015
 * Time: 10:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Telemonitor
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.trayMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.trayMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// trayIcon
			// 
			this.trayIcon.ContextMenuStrip = this.trayMenu;
			this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
			this.trayIcon.Text = "Telemonitor";
			this.trayIcon.Visible = true;
			// 
			// trayMenu
			// 
			this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.trayMenuItemExit});
			this.trayMenu.Name = "trayMenu";
			this.trayMenu.Size = new System.Drawing.Size(109, 26);
			// 
			// trayMenuItemExit
			// 
			this.trayMenuItemExit.Name = "trayMenuItemExit";
			this.trayMenuItemExit.Size = new System.Drawing.Size(108, 22);
			this.trayMenuItemExit.Text = "Выход";
			this.trayMenuItemExit.Click += new System.EventHandler(this.TrayMenuItemExitClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(124, 0);
			this.Name = "MainForm";
			this.Text = "Telemonitor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
			this.Load += new System.EventHandler(this.MainFormLoad);
			this.Shown += new System.EventHandler(this.MainFormShown);
			this.trayMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.ToolStripMenuItem trayMenuItemExit;
		private System.Windows.Forms.ContextMenuStrip trayMenu;
		private System.Windows.Forms.NotifyIcon trayIcon;
		
		
		
		void TrayMenuItemExitClick(object sender, System.EventArgs e)
		{
			Close();
		}
						
	}
}
