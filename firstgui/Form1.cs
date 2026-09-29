using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mousejiggler
{
    public partial class Form1 : Form
    {
        //tracks whether jiggler is active
        private bool isJiggling = false;

        //track which direction to nudge and how far
        private int nudgeDirection = 30;

        //defining the timer
        private Timer timer1 = new Timer();
        private int secondsElapsed = 0;
        private Button myButton = new Button();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo); //importing the Windows API to inject global keyboard events

        //importing native API for window selection
        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

        private const byte VK_SHIFT = 0x10;
        private const uint KEYEVENTF_KEYDOWN = 0X000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private Label statusLabel;
        private Label timerLabel;



        public Form1()
        {
            InitializeComponent();          //for the love of god please don't touch these I spent so long trying to get them to not break
            SetupCustomUI();

            this.TopMost = true; //keep the window on top of other windows
        }

        private void SetupCustomUI()
        {
            //Windows Layout
            this.Text = "Mouse Jiggler";
            this.Size = new Size(250, 150);
            this.FormBorderStyle = FormBorderStyle.FixedSingle; //Prevent resizing
            this.MinimizeBox = false; //Disable miximize button
            this.MaximizeBox = false; //Disable maximize button

            //I'm wanting to avoid a scenario where the button gets lost or it takes over the whole screen


            this.Controls.Add(myButton);    //adding Button UI control to the container

            //Button layout
            myButton.Text = "Start Jiggler";
            myButton.Size = new Size(160, 45);  //set button size on screen, currently relatively small resolution
            myButton.Location = new Point(35, 35); //Center button on window
            myButton.Font = new Font("Arial", 10, FontStyle.Bold); //setting font style for the button, set to bold
            myButton.BackColor = Color.LightGreen; //setting the button to light green on start up
            myButton.Click += MyButton_Click; //attaching a handler method to the button



            timer1.Interval = 240000; //shake intervals in milliseconds
            timer1.Tick += Timer1_Tick;
            timer1.Enabled = false; //stop timer from starting as soon as app is opened




            //creating text in form tracking activity
            statusLabel = new Label();
            statusLabel.Text = "Waiting for the first jiggle...";
            statusLabel.Location = new System.Drawing.Point(20, 20); //positioning text on window
            statusLabel.AutoSize = true;



            this.Controls.Add(statusLabel); //printing the text into the window
            this.Controls.Add(timerLabel);



        }

        private void MyButton_Click(object sender, EventArgs e)     //this is the method that was called earlier, when the button is clicked this is what runs
        {
            if (!isJiggling)
            {
                //Start Jiggling
                isJiggling = true;
                timer1.Start();
                myButton.Text = "Stop Jiggler";
                myButton.BackColor = Color.Red;


            }
            else
            {
                //Stop Jiggling
                isJiggling = false;
                timer1.Stop();
                myButton.Text = "Start Jiggler";
                myButton.BackColor = Color.LightGreen;


            }

        }

        //nudge mouse around
        private async void Timer1_Tick(object sender, EventArgs e)
        {

            Point currentPosition = Cursor.Position;    //find mouse coordinates on x and y axis

            int nextX = currentPosition.X + nudgeDirection; //calculate where on the horizontal plane the mouse is going

            Cursor.Position = new Point(nextX, currentPosition.Y);  //move the moues around the x coordinate, keep the current y coordinate

            nudgeDirection = -nudgeDirection;       //shoves the mouse around left and right

            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYDOWN, 0);

            //inputting a global shift key press directly into the OS
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);


            statusLabel.Text = $"Last Jiggle: {DateTime.Now.ToLongTimeString()}";


            IntPtr shell = GetShellWindow();
            SetForegroundWindow(shell);             //deselect window in focus

            await Task.Delay(1000);
            IntPtr previous = GetForegroundWindow(); //get the window that was in focus before the shell window was selected



            SetForegroundWindow(previous); //return focus to the previous window




        }
    }

}


