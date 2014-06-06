namespace Notification
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tNotify = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tNotify
            // 
            this.tNotify.Location = new System.Drawing.Point(0, 0);
            this.tNotify.Multiline = true;
            this.tNotify.Name = "tNotify";
            this.tNotify.ReadOnly = true;
            this.tNotify.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tNotify.Size = new System.Drawing.Size(740, 314);
            this.tNotify.TabIndex = 0;
            this.tNotify.WordWrap = false;
            this.tNotify.TextChanged += new System.EventHandler(this.tNotify_TextChanged);
            this.tNotify.DoubleClick += new System.EventHandler(this.tNotify_DoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 320);
            this.Controls.Add(this.tNotify);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tNotify;
    }
}

