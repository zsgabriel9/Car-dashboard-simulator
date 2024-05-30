using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Auto_simulacia
{
    public partial class Form1 : Form
    {
        private int hladina_nadrze = 100000, bateria = 10000;

        private int aktualneRPM, cieloveRPM, hodnotaSmerovky, Svetlo, SvetloPredchadzajuce;
        private Int16 smerovka_stav = 0, smerovkaPredoslyStav = 0;
        private double teplotaMotora = 20, ventilator = 0, ventilatorUhol = 0, teplotaUhol = 0, teplota = 0, teplotaKabiny = 20, zmenaTeploty0, zmenaTeploty;
        private bool jeOtacany = false, Brzda;
        private Point poslednaPoziciaMysi;
        
        Image maleLogo = new Bitmap(100, 100);
        private Motor autoMotor = new Motor();
        private Automobil auto = new Automobil();






        public Form1()
        {
            InitializeComponent();

            auto.EventSmerovky += new Automobil.EventSmerovkyHandler(Blikanie_Smerovky);

            //Zaoblenie panelu
            System.Drawing.Drawing2D.GraphicsPath path0 = new System.Drawing.Drawing2D.GraphicsPath();
            int velkostKrivky = 50;
            int priemer = velkostKrivky * 2;
            Rectangle obdlznik = new Rectangle(0, 0, panel1.Width, panel1.Height - velkostKrivky);
            path0.AddArc(0, 0, priemer, priemer, 180, 90);
            path0.AddLine(velkostKrivky, 0, panel1.Width - velkostKrivky, 0);
            path0.AddArc(panel1.Width - priemer, 0, priemer, priemer, 270, 90);
            path0.AddLine(panel1.Width, velkostKrivky, panel1.Width, panel1.Height - velkostKrivky);
            path0.AddArc(panel1.Width - priemer, panel1.Height - priemer, priemer, priemer, 0, 90);
            path0.AddLine(panel1.Width - velkostKrivky, panel1.Height, velkostKrivky, panel1.Height);
            path0.AddArc(0, panel1.Height - priemer, priemer, priemer, 90, 90);
            path0.CloseAllFigures();
            panel1.Region = new System.Drawing.Region(path0);

            // Výpočet umiestnenia palubovky
            /*
            Point pozPalubovka = new Point((ClientSize.Width - panel1.Width) / 2, (ClientSize.Height - panel1.Height) / 2);
            Console.WriteLine("Vypočítaná poloha X panela 1 je: " + pozPalubovka.ToString());
            */

            // Vytvorenie kruhovej dráhy GraphicsPath 1
            System.Drawing.Drawing2D.GraphicsPath path1 = new System.Drawing.Drawing2D.GraphicsPath();
            path1.AddEllipse(0, 0, pictureBox1.Width, pictureBox1.Height);
            // Vytvorenie kruhovej dráhy GraphicsPath 2
            System.Drawing.Drawing2D.GraphicsPath path2 = new System.Drawing.Drawing2D.GraphicsPath();
            path2.AddEllipse(0, 0, pictureBox2.Width, pictureBox2.Height);
            // Vytvorenie kruhovej dráhy GraphicsPath 3
            System.Drawing.Drawing2D.GraphicsPath path3 = new System.Drawing.Drawing2D.GraphicsPath();
            path3.AddEllipse(0, 0, pictureBox3.Width, pictureBox3.Height);
            // Vytvorenie kruhovej dráhy GraphicsPath 4
            System.Drawing.Drawing2D.GraphicsPath path4 = new System.Drawing.Drawing2D.GraphicsPath();
            path4.AddEllipse(0, 0, pictureBox4.Width, pictureBox4.Height);

            // Nastavenie okna pictureBox1 na kruhovú dráhu GraphicsPath 1
            pictureBox1.Region = new System.Drawing.Region(path1);
            // Nastavenie okna pictureBox2 na kruhovú dráhu GraphicsPath 2
            pictureBox2.Region = new System.Drawing.Region(path2);
            // Nastavenie okna pictureBox2 na kruhovú dráhu GraphicsPath 3
            pictureBox3.Region = new System.Drawing.Region(path3);
            // Nastavenie okna pictureBox2 na kruhovú dráhu GraphicsPath 2
            pictureBox4.Region = new System.Drawing.Region(path4);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //Vypnutie okrajov button1 pri interakcii
            button1.FlatAppearance.MouseOverBackColor = button1.BackColor;
            button1.FlatAppearance.MouseDownBackColor = button1.BackColor;
            button6.FlatAppearance.MouseOverBackColor = button1.BackColor;
            button6.FlatAppearance.MouseDownBackColor = button1.BackColor;

            // Načítanie obrázka
            Image logo = Image.FromFile("Images/palivo_logo.jpg");

            // Zmena veľkosti obrázka na menšiu veľkosť
            using (Graphics g2 = Graphics.FromImage(maleLogo))
            {
                g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g2.DrawImage(logo, 35, 60, 25, 25);
            }


            // Načítanie obrázka
            Image logo1 = Image.FromFile("Images/voda_logo.png");

            // Zmena veľkosti obrázka na menšiu veľkosť
            Image maleLogo1 = new Bitmap(100, 100);
            using (Graphics g2 = Graphics.FromImage(maleLogo1))
            {
                g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g2.DrawImage(logo1, 32, 57, 25, 25);
            }


            // Nastavenie obrázka do pictureBox2
            pictureBox2.Image = maleLogo;
            pictureBox3.Image = maleLogo1;

        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e) //OTÁČKOMER//
        {

            // Inicializácia grafiky a rozmerov otáčkomera
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int stredX = pictureBox1.Width / 2;
            int stredY = pictureBox1.Height / 2;
            int otackomerRadius = pictureBox1.Height / 2 - 10;
            int dlzkaIhly = otackomerRadius - 20;

            // Vytvorenie pier na vyxkreslenie prvkov otáčkomera
            Pen pen1 = new Pen(Color.Black, 4);
            Pen pen2 = new Pen(Color.Red, 4);

            // Vykreslenie čierneho kruhu na základni ihly
            SolidBrush brush1 = new SolidBrush(Color.Black);
            int kruhRadius = 6;
            int kruhX = stredX - kruhRadius;
            int kruhY = stredY - kruhRadius;
            g.FillEllipse(brush1, kruhX, kruhY, kruhRadius * 2, kruhRadius * 2);

            // Výpočet polohy každého označenia na otáčkomeri
            for (int i = -4; i <= 3; i++)
            {
                int uhol = i * 35;
                int dlzkaPera = 10;
                if (i % 5 == 0)
                {
                    dlzkaPera = 12;
                }
                int x1 = stredX + (int)(otackomerRadius * Math.Sin(uhol * Math.PI / 180));
                int y1 = stredY - (int)(otackomerRadius * Math.Cos(uhol * Math.PI / 180));
                int x2 = stredX + (int)((otackomerRadius - dlzkaPera) * Math.Sin(uhol * Math.PI / 180));
                int y2 = stredY - (int)((otackomerRadius - dlzkaPera) * Math.Cos(uhol * Math.PI / 180));

                //Pridanie čísiel
                string cislo = (i + 4).ToString(); // Určenie čísiel na otáčkomeri
                int cisloX = stredX - 6 + (int)((otackomerRadius - 25) * Math.Sin(uhol * Math.PI / 180));
                int cisloY = stredY - 6 - (int)((otackomerRadius - 25) * Math.Cos(uhol * Math.PI / 180));
                g.DrawString(cislo, new Font("Arial", 12), Brushes.Black, cisloX, cisloY);
                g.DrawString("RPM \nx1000", new Font("Arial", 11), Brushes.Black, 90, 150);

                if (i == 2)
                {
                    g.DrawArc((new Pen(Color.Red, 7)), new Rectangle(stredX - otackomerRadius + 5, stredY - otackomerRadius + 5, (otackomerRadius - 5) * 2, (otackomerRadius - 5) * 2), 340, 35);
                }

                g.DrawLine(pen1, x1, y1, x2, y2);

                if (i == -4)
                {
                    g.DrawArc(pen1, new Rectangle(stredX - otackomerRadius, stredY - otackomerRadius, otackomerRadius * 2, otackomerRadius * 2), -231, 247);
                }

                g.DrawLine(pen1, x1, y1, x2, y2);
            }



            // vykreslenie ihly na otáčkomeri
            int uholStupne = (int)(260.0 * 64 * aktualneRPM / 60 / 8000.0 - 140.0);
            int x = stredX + (int)(dlzkaIhly * Math.Sin(uholStupne * Math.PI / 180));
            int y = stredY - (int)(dlzkaIhly * Math.Cos(uholStupne * Math.PI / 180));
            g.DrawLine(pen2, stredX, stredY, x, y);
        }


        private void pictureBox2_Paint(object sender, PaintEventArgs e) //UKAZOVATEĽ PALIVA//
        {


            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int stredX = pictureBox2.Width / 2;
            int stredY = pictureBox2.Height / 2;
            int Radius = pictureBox2.Height / 2 - 10;
            int palivoDlzkaIhly = Radius - 10;

            SolidBrush brush1 = new SolidBrush(Color.Black);
            int kruhRadius = 6;
            int kruhX = stredX - kruhRadius;
            int kruhY = stredY - kruhRadius;
            g.FillEllipse(brush1, kruhX+3, kruhY+3, kruhRadius, kruhRadius);

            g.DrawString("F", new Font("Arial", 7), Brushes.Black, 60, 60);
            g.DrawString("E", new Font("Arial", 7), Brushes.Black, 20, 60);

            Pen pen1 = new Pen(Color.Black, 1);
            for (int i = -4; i <= 4; i++)
            {
                int uhol = i * 30;
                int dlzkaPera = 10;
                if (i % 5 == 0)
                {
                    dlzkaPera = 20;
                }
                int x1 = stredX + (int)(Radius * Math.Sin(uhol * Math.PI / 180));
                int y1 = stredY - (int)(Radius * Math.Cos(uhol * Math.PI / 180));
                int x2 = stredX + (int)((Radius - dlzkaPera) * Math.Sin(uhol * Math.PI / 180));
                int y2 = stredY - (int)((Radius - dlzkaPera) * Math.Cos(uhol * Math.PI / 180));
                g.DrawLine(pen1, x1, y1, x2, y2);
            }

            Pen pen2 = new Pen(Color.Blue, 2);
            int palivoUholStupne = (int)(240.0 / 1000 * hladina_nadrze / 100.0 - 120.0);
            int palivoX = stredX + (int)(palivoDlzkaIhly * Math.Sin(palivoUholStupne * Math.PI / 180));
            int palivoY = stredY - (int)(palivoDlzkaIhly * Math.Cos(palivoUholStupne * Math.PI / 180));
            g.DrawLine(pen2, stredX, stredY, palivoX, palivoY);
        }


        private void pictureBox3_Paint(object sender, PaintEventArgs e)//UKAZOVATEĽ TEPLOTY VODY MOTORA
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int stredX = pictureBox2.Width / 2;
            int stredY = pictureBox2.Height / 2;
            int Radius = pictureBox2.Height / 2 - 10;
            int teplotaDlzkaIhly = Radius - 10;

            SolidBrush brush1 = new SolidBrush(Color.Black);
            int kruhRadius = 6;
            int kruhX = stredX - kruhRadius;
            int kruhY = stredY - kruhRadius;
            g.FillEllipse(brush1, kruhX + 3, kruhY + 3, kruhRadius, kruhRadius);

            g.DrawString("°C", new Font("Arial", 7), Brushes.Black, 30, 60);

            Pen pen1 = new Pen(Color.Black, 1);
            for (int i = -4; i <= 4; i++)
            {
                int uhol = i * 30;
                int dlzkaPera = 10;
                if (i % 5 == 0)
                {
                    dlzkaPera = 20;
                }
                int x1 = stredX + (int)(Radius * Math.Sin(uhol * Math.PI / 180));
                int y1 = stredY - (int)(Radius * Math.Cos(uhol * Math.PI / 180));
                int x2 = stredX + (int)((Radius - dlzkaPera) * Math.Sin(uhol * Math.PI / 180));
                int y2 = stredY - (int)((Radius - dlzkaPera) * Math.Cos(uhol * Math.PI / 180));
                g.DrawLine(pen1, x1, y1, x2, y2);

                string cislo = (((i + 4) * 10 + 10)+40).ToString(); // Určenie čísiel na otáčkomeri
                int cisloX = stredX - 6 + (int)((Radius + 2) * Math.Sin((uhol + 10) * Math.PI / 180));
                int cisloY = stredY - 6 - (int)((Radius + 2) * Math.Cos((uhol + 10) * Math.PI / 180));
                g.DrawString(cislo, new Font("Arial", 6), Brushes.Black, cisloX, cisloY);
            }

            Pen pen2 = new Pen(Color.Blue, 2);
            int UholStupne;
            if (teplotaMotora >= 50)
                UholStupne = (int)(3 * teplotaMotora - 271.0);
            else
                UholStupne = (int)(-120);
            int teplotaX = stredX + (int)(teplotaDlzkaIhly * Math.Sin(UholStupne * Math.PI / 180));
            int teplotaY = stredY - (int)(teplotaDlzkaIhly * Math.Cos(UholStupne * Math.PI / 180));
            g.DrawLine(pen2, stredX, stredY, teplotaX, teplotaY);
        }


        private void pictureBox4_Paint(object sender, PaintEventArgs e)//TACHOMETER(BEZ FUNKCIE)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int stredX = pictureBox1.Width / 2;
            int stredY = pictureBox1.Height / 2;
            int tachometerRadius = pictureBox1.Height / 2 - 10;
            int dlzkaIhly = tachometerRadius - 20;

            Pen pen1 = new Pen(Color.Black, 4);
            Pen pen2 = new Pen(Color.Red, 4);
            Pen pen3 = new Pen(Color.Black, 1);

            SolidBrush brush1 = new SolidBrush(Color.Black);
            int kruhRadius = 6;
            int kruhX = stredX - kruhRadius;
            int kruhY = stredY - kruhRadius;
            g.FillEllipse(brush1, kruhX, kruhY, kruhRadius * 2, kruhRadius * 2);

            // Hrubé čiary
            for (int i = -7; i <= 6; i++)
            {
                int uhol = i * 20;
                int dlzkaPera = 10;
                if (i % 5 == 0)
                {
                    dlzkaPera = 12;

                }
                int x1 = stredX + (int)(tachometerRadius * Math.Sin(uhol * Math.PI / 180));
                int y1 = stredY - (int)(tachometerRadius * Math.Cos(uhol * Math.PI / 180));
                int x2 = stredX + (int)((tachometerRadius - dlzkaPera) * Math.Sin(uhol * Math.PI / 180));
                int y2 = stredY - (int)((tachometerRadius - dlzkaPera) * Math.Cos(uhol * Math.PI / 180));

                string cislo = ((i + 7) * 20).ToString();
                int cisloX = stredX - 10 + (int)((tachometerRadius - 25) * Math.Sin(uhol * Math.PI / 180));
                int cisloY = stredY - 8 - (int)((tachometerRadius - 25) * Math.Cos(uhol * Math.PI / 180));
                g.DrawString(cislo, new Font("Arial", 9), Brushes.Black, cisloX, cisloY);
                g.DrawString("km/h", new Font("Arial", 11), Brushes.Black, 90, 160);

                g.DrawLine(pen1, x1, y1, x2, y2);

                if (i == -4)
                {
                    g.DrawArc(pen1, new Rectangle(stredX - tachometerRadius, stredY - tachometerRadius, tachometerRadius * 2, tachometerRadius * 2), -231, 262);
                }

                g.DrawLine(pen1, x1, y1, x2, y2);
            }

            // Tenké čiary
            for (int i = -6; i <= 6; i++)
            {
                int uhol = (i * 20) - 10;
                int dlzkaPera = 10;
                if (i % 5 == 0)
                {
                    dlzkaPera = 12;

                }
                int x1 = stredX + (int)(tachometerRadius * Math.Sin(uhol * Math.PI / 180));
                int y1 = stredY - (int)(tachometerRadius * Math.Cos(uhol * Math.PI / 180));
                int x2 = stredX + (int)((tachometerRadius - dlzkaPera) * Math.Sin(uhol * Math.PI / 180));
                int y2 = stredY - (int)((tachometerRadius - dlzkaPera) * Math.Cos(uhol * Math.PI / 180));

                g.DrawLine(pen3, x1, y1, x2, y2);
            }

            int uholStupne = -140;
            int x = stredX + (int)(dlzkaIhly * Math.Sin(uholStupne * Math.PI / 180));
            int y = stredY - (int)(dlzkaIhly * Math.Cos(uholStupne * Math.PI / 180));
            g.DrawLine(pen2, stredX, stredY, x, y);
        }


        private void trackBar1_Scroll(object sender, EventArgs e) //PLYNOVÝ PEDÁL//
        {

            if (autoMotor.JeZapnuty)
            {
                if (hladina_nadrze <= 0)
                    cieloveRPM = 0;
                else
                {
                    cieloveRPM = trackBar1.Value;
                }
            }
            else
            {
                cieloveRPM = 0;
            }
            pictureBox1.Invalidate();

        }


        private void trackBar2_Scroll(object sender, EventArgs e) //BRZDOVÝ PEDÁL//
        {
            if (bateria > 0)
            {
                if (trackBar2.Value > 0)
                    Brzda = true;
                else
                    Brzda = false;
            }
        }


        private void timer1_Tick(object sender, EventArgs e)//MULTIFUNKČNÝ ČASOVAČ S INTERVALOM 1ms
        {
            // Zníženie hladiny paliva a regulácia teploty motora na základe času
            if (autoMotor.JeZapnuty)
            {
                button6.Image = Image.FromFile("Images/bateria1.png");
                hladina_nadrze -= (aktualneRPM / 250);
                if (hladina_nadrze <= 0)
                {
                    hladina_nadrze = 0;
                    cieloveRPM = 0;
                    autoMotor.Stop();

                    //Otočenie kľúča to vypnutej polohy
                    button1.Image.RotateFlip(RotateFlipType.Rotate270FlipNone); // otočenie kľúča o 90 stupňov
                    button1.Invalidate(); // obnovenie tlačidla na zobrazenie otočeného kľúća
                }
                if (teplotaMotora < 90)
                    teplotaMotora += Convert.ToDouble(aktualneRPM) / 250000;
                else
                    teplotaMotora = 90;
            }
            else
            {
                button6.Image = Image.FromFile("Images/bateria0.png");
                if (teplotaMotora > 20)
                    teplotaMotora -= 0.0002;
                else
                    teplotaMotora = 20;
            }


            //Výpočet zmeny RPM
            int rpmRozdiel = cieloveRPM - aktualneRPM;
            int rpmPrirastok = (int)Math.Sign(rpmRozdiel) * Math.Min(50, Math.Abs(rpmRozdiel));
            aktualneRPM += rpmPrirastok;
            //Obmedzovač otáčok
            if (aktualneRPM == 6500)
                aktualneRPM = 6000;

            // Výpis label-ov
            label1.Text = "Hladina nádrže: " + (hladina_nadrze / 1000).ToString() + " %";
            label2.Text = (aktualneRPM).ToString() + " RPM";
            label3.Text = "Teplota vody: " + teplotaMotora.ToString("F2") + " °C";
            label4.Text = "Úroveň batérie: " + Convert.ToString(bateria / 100) + " %";
            label8.Text = "Vnútoná teplota: " +(teplotaKabiny).ToString("F2") + " °C";

            // Aktualizácia vykreslenia prvkov
            UpdateImage_Front();
            UpdateImage_Back();
            pictureBox1.Invalidate();
            pictureBox2.Invalidate();
            pictureBox3.Invalidate();
            pictureBox5.Invalidate();
            pictureBox6.Invalidate();

            // Nastavenie obráźka do pictureBox2 pri nízkej hladine paliva
            if (hladina_nadrze < 13000)
            {
                // Načítanie obrázka
                Image logo1 = Image.FromFile("Images/palivo_logo_svieti.jpg");

                // Zmenšenie obrázka
                Image maleLogo1 = new Bitmap(100, 100);
                using (Graphics g2 = Graphics.FromImage(maleLogo1))
                {
                    g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g2.DrawImage(logo1, 35, 60, 25, 25);
                }
                pictureBox2.Image = maleLogo1;

            }
            else
                pictureBox2.Image = maleLogo;


            //Vypnutie prvkov pri vybitej batérii
            if (bateria <= 0)
            {
                Svetlo = 0;
                Brzda = false;
                button6.Image = Image.FromFile("Images/bateria1.png");
                button4.Image = Image.FromFile("Images/svetlo0.png");
                pictureBox2.Image = maleLogo;
            }
            // Nastavnie svetla na stav pred vybitím batérie
            else
            {
                Svetlo = SvetloPredchadzajuce;
                if (SvetloPredchadzajuce == 1)
                    button4.Image = Image.FromFile("Images/svetlo1.png");
                else if (SvetloPredchadzajuce == 2)
                    button4.Image = Image.FromFile("Images/svetlo2.png");
                else
                    button4.Image = Image.FromFile("Images/svetlo0.png");
            }
               



        }


        private void timer2_Tick(object sender, EventArgs e)//VYBÍJANIE A NABíJANIE BATÉRIE S INTERVALOM 1s
        {
            if (autoMotor.JeZapnuty)
            {
                if (bateria < 10000)
                    bateria += aktualneRPM / 120;
                else if (bateria > 10000)
                    bateria = 10000;
            }
            else
            {
                if (bateria <= 0)
                    bateria = 0;
                else
                {
                    if (smerovka_stav != 0 && smerovka_stav != 2)
                        bateria -= 3;
                    else if (smerovka_stav == 2)
                        bateria -= 6;
                    if (Svetlo == 1)
                        bateria -= 5;
                    else if (Svetlo == 2)
                        bateria -= 7;
                    if (Brzda == true)
                        bateria -= 3;
                    if (ventilator == 20)
                        bateria -= 1;
                    else if (ventilator == 40)
                        bateria -= 2;
                    else if (ventilator == 60)
                        bateria -= 3;
                    else if (ventilator == 80)
                        bateria -= 4;
                }
            }
        }


        private async void button1_MouseDown(object sender, MouseEventArgs e)//OVLÁDANIE KĽÚČA
        {
            if (bateria >= 250)
            {
                if (autoMotor.JeZapnuty)
                {
                    button1.Image.RotateFlip(RotateFlipType.Rotate90FlipNone); // rotate the image by 90 degrees
                    button1.Invalidate(); // refresh the button to show the rotated image

                    // Stop the engine
                    autoMotor.Stop();
                    cieloveRPM = 0;
                }
                else
                {
                    button1.Image.RotateFlip(RotateFlipType.Rotate270FlipNone); // rotate the image by 90 degrees
                    button1.Invalidate(); // refresh the button to show the rotated image

                    // Engine is not running, so start it
                    if (hladina_nadrze <= 0)
                    {
                        Console.WriteLine("Palivová nádrž je prázdna");
                        await Task.Delay(500);
                        button1.Image.RotateFlip(RotateFlipType.Rotate270FlipNone); // rotate the image by 90 degrees
                        button1.Invalidate(); // refresh the button to show the rotated image

                    }

                    else
                    {
                        autoMotor.Start();
                        cieloveRPM = trackBar1.Value;
                    }
                    bateria -= 250;
                }
            }
            else
            {
                Console.WriteLine("Vybitá batéria");
                button1.Image.RotateFlip(RotateFlipType.Rotate90FlipNone); // rotate the image by 90 degrees
                button1.Invalidate(); // refresh the button to show the rotated image
                await Task.Delay(500);
                button1.Image.RotateFlip(RotateFlipType.Rotate270FlipNone); // rotate the image by 90 degrees
                button1.Invalidate(); // refresh the button to show the rotated image
            }
        }


        protected class Motor//TRIEDA PRE STAV MOTORA
        {

            private bool jeZapnuty;

            public bool JeZapnuty
            {
                get { return jeZapnuty; }
            }

            public void Start()
            {
                if (!jeZapnuty)
                {
                    jeZapnuty = true;
                    Console.WriteLine("Motor naštartovaný");
                }
            }

            public void Stop()
            {
                if (jeZapnuty)
                {
                    jeZapnuty = false;
                    Console.WriteLine("Motor vypnutý");
                }
            }
        }




        protected class Automobil//TRIEDA POUŽITÁ PRE LOGIKU SMEROVIEK
        {
            private Int16 Smerovka;
            private SmerovkyEventArgs SmerovkyEventArgs;
            private Timer Časovač_Smerovky;
            public event EventSmerovkyHandler EventSmerovky;
            public delegate void EventSmerovkyHandler(object sender, SmerovkyEventArgs e);

            public Automobil()
            {
                this.Smerovka = 0;
                SmerovkyEventArgs = new SmerovkyEventArgs();
                Časovač_Smerovky = new Timer();
                Časovač_Smerovky.Interval = 750;
                Časovač_Smerovky.Tick += new EventHandler(Časovač_Tick);
            }

            public void Časovač_Tick(object sender, EventArgs e)
            {
                if (this.Smerovka == -1)
                {
                    SmerovkyEventArgs.Ľavá_Smerovka = !SmerovkyEventArgs.Ľavá_Smerovka;
                }
                else if (this.Smerovka == 1)
                {
                    SmerovkyEventArgs.Pravá_Smerovka = !SmerovkyEventArgs.Pravá_Smerovka;
                }
                else if (this.Smerovka == 2)
                {
                    SmerovkyEventArgs.Výstražné = !SmerovkyEventArgs.Výstražné;
                }
                else
                {
                    SmerovkyEventArgs.Pravá_Smerovka = false;
                    SmerovkyEventArgs.Ľavá_Smerovka = false;
                    SmerovkyEventArgs.Výstražné = false;
                    Časovač_Smerovky.Enabled = false;
                }
                if (EventSmerovky != null)
                {
                    EventSmerovky(this, SmerovkyEventArgs);
                }

            }

            public Int16 mSmerovka(Int16 Smerovka)
            {
                if (Smerovka < 0)
                {
                    this.Smerovka = -1;
                    Časovač_Smerovky.Enabled = true;
                    SmerovkyEventArgs.Ľavá_Smerovka = true;
                    SmerovkyEventArgs.Pravá_Smerovka = false;
                    SmerovkyEventArgs.Výstražné = false;
                }


                else if (Smerovka == 1)
                {
                    this.Smerovka = 1;
                    Časovač_Smerovky.Enabled = true;
                    SmerovkyEventArgs.Ľavá_Smerovka = false;
                    SmerovkyEventArgs.Pravá_Smerovka = true;
                    SmerovkyEventArgs.Výstražné = false;
                }

                else if (Smerovka == 2)
                {
                    this.Smerovka = 2;
                    Časovač_Smerovky.Enabled = true;
                    SmerovkyEventArgs.Ľavá_Smerovka = SmerovkyEventArgs.Pravá_Smerovka = false;
                    SmerovkyEventArgs.Výstražné = true;
                }

                else this.Smerovka = 0;
                if (EventSmerovky != null)
                {
                    EventSmerovky(this, SmerovkyEventArgs);
                }
                return this.Smerovka;

            }


        }


        public void Blikanie_Smerovky(object sender, SmerovkyEventArgs e)//NASTAVENIE HODNOTY SMEROVKY PRE VYKRESLENIE
        {
            if (bateria > 0)
            {
                if (e.Pravá_Smerovka)
                {
                    button3.Image = Image.FromFile("Images/smerovka_doprava_on.png");
                    hodnotaSmerovky = 1;
                }
                else if (e.Ľavá_Smerovka)
                {
                    button2.Image = Image.FromFile("Images/smerovka_dolava_on.png");
                    hodnotaSmerovky = -1;
                }
                else if (e.Výstražné)
                {
                    button2.Image = Image.FromFile("Images/smerovka_dolava_on.png");
                    button3.Image = Image.FromFile("Images/smerovka_doprava_on.png");
                    hodnotaSmerovky = 2;
                }
                else
                {
                    button2.Image = Image.FromFile("Images/smerovka_dolava_off.png");
                    button3.Image = Image.FromFile("Images/smerovka_doprava_off.png");
                    hodnotaSmerovky = 0;
                }
            }
            else
            {
                hodnotaSmerovky = 0;
                button2.Image = Image.FromFile("Images/smerovka_dolava_off.png");
                button3.Image = Image.FromFile("Images/smerovka_doprava_off.png");
            }
        }


        private void button2_Click(object sender, EventArgs e)//OVLÁDANIE SMEROVKY DOLAVA
        {
            if (smerovka_stav == 2)
            {
                if (smerovkaPredoslyStav == -1)
                    smerovkaPredoslyStav = 0;
                else
                    smerovkaPredoslyStav = -1;
            }
            else if (smerovka_stav != -1 && smerovka_stav != 2)
            {
                smerovka_stav = -1;
                smerovkaPredoslyStav = smerovka_stav;
                auto.mSmerovka(smerovka_stav);
            }
            else
            {
                smerovka_stav = 0;
                smerovkaPredoslyStav = smerovka_stav;
                auto.mSmerovka(smerovka_stav);
            }
        }


        private void button3_Click(object sender, EventArgs e)//OVLÁDANIE SMEROVKY DOPRAVA
        {
            if (smerovka_stav == 2)
            {
                if (smerovkaPredoslyStav == 1)
                    smerovkaPredoslyStav = 0;
                else
                    smerovkaPredoslyStav = 1;
            }
            else if (smerovka_stav != 1 && smerovka_stav != 2)
            {
                smerovka_stav = 1;
                smerovkaPredoslyStav = smerovka_stav;
                auto.mSmerovka(smerovka_stav);
            }
            else
            {
                smerovka_stav = 0;
                smerovkaPredoslyStav = smerovka_stav;
                auto.mSmerovka(smerovka_stav);
            }
        }


        private void button5_Click(object sender, EventArgs e)//OVLADANIE VÝSTRAŽNÝCH SVETIEL
        {
            if (smerovka_stav != 2)
            {
                smerovka_stav = 2;
                auto.mSmerovka(smerovka_stav);
            }
            else
            {
                smerovka_stav = smerovkaPredoslyStav;
                auto.mSmerovka(smerovka_stav);
            }
        }


        public class SmerovkyEventArgs : EventArgs
        {
            public bool Ľavá_Smerovka, Pravá_Smerovka, Výstražné;
            //public int Svetlo;
        }


        private void button4_Click(object sender, EventArgs e)//OVLÁDANIE HLAVNÝCH SVETIEL
        {
            if (bateria > 0)
            {
                if (Svetlo == 0)
                {
                    Svetlo = SvetloPredchadzajuce = 1;
                    button4.Image = Image.FromFile("Images/svetlo1.png");
                }
                else if (Svetlo == 1)
                {
                    Svetlo = SvetloPredchadzajuce = 2;
                    button4.Image = Image.FromFile("Images/svetlo2.png");
                }
                else
                {
                    Svetlo = SvetloPredchadzajuce = 0;
                    button4.Image = Image.FromFile("Images/svetlo0.png");
                }
            }
        }


        private void UpdateImage_Front()//VYKRESLENIE SVEIEL VPREDU
        {
            // Načítanie pôvodného obrázka
            Bitmap povodnyObrazok = new Bitmap("Images/auto_predok.png");

            // Vytvorenie novej bitovej mapy s rovnakými rozmermi ako pôvodný obrázok
            Bitmap novyObrazok = new Bitmap(povodnyObrazok.Width, povodnyObrazok.Height);

            // Kreslenie pôvodného obrázka na novú bitovú mapu
            using (Graphics g = Graphics.FromImage(novyObrazok))
            {
                g.DrawImage(povodnyObrazok, new Rectangle(0, 0, novyObrazok.Width, novyObrazok.Height));

                ////SMEROVKY VPREDU/////
                // Maľovanie oblasti s radiálnym gradientom, ak hodnota Smerovky nie je 0
                if (hodnotaSmerovky != 0)
                {
                    // Nastavenie pozície X pre svetlá
                    int svetloX1 = 0, svetloX2 = 0, vyskaSvetla = 0;
                    if (hodnotaSmerovky == -1)
                        svetloX1 = 650;
                    else if (hodnotaSmerovky == 1)
                        svetloX1 = 90;
                    if (hodnotaSmerovky == 2)
                    {
                        svetloX1 = 90;
                        svetloX2 = 650;
                    }

                    // Definovanie veľkosti a polohy štvorca gradientu
                    int sirkaSvetla = vyskaSvetla = 100; // Width and Height of the light
                    int svetloY = 310; // Y position of the light

                    // Definovanie štvorca, v ktorom sa nachádza oblasť, ktorá sa má vyplniť gradientom
                    RectangleF gradientRect1 = new RectangleF(svetloX1, svetloY, sirkaSvetla, vyskaSvetla);
                    RectangleF gradientRect2 = new RectangleF(svetloX2, svetloY, sirkaSvetla, vyskaSvetla);

                    // Vytvorenie nového PathGradientBrush, ktorý používa kruhovú cestu na definovanie gradientu
                    GraphicsPath gradientPath1 = new GraphicsPath();
                    gradientPath1.AddEllipse(gradientRect1);
                    PathGradientBrush gradientBrush1 = new PathGradientBrush(gradientPath1);

                    GraphicsPath gradientPath2 = new GraphicsPath();
                    gradientPath2.AddEllipse(gradientRect2);
                    PathGradientBrush gradientBrush2 = new PathGradientBrush(gradientPath2);

                    // Nastavenie stredovej farby gradientu na úplne nepriehľadnú oranžovú farbu
                    gradientBrush1.CenterColor = gradientBrush2.CenterColor = Color.Orange;

                    // Definovanie poľa farieb od úplne priehľadnej po úplne nepriehľadnú
                    Color[] farba = { Color.FromArgb(0, Color.Orange), Color.FromArgb(32, Color.Orange) };

                    // Nastavenie okolitých farieb gradientu na pole farieb
                    gradientBrush1.SurroundColors = gradientBrush2.SurroundColors = farba;

                    // Vyplnenie elipsy pomocou gradientného štetca
                    if (hodnotaSmerovky == 2)
                    {
                        g.FillEllipse(gradientBrush1, gradientRect1);
                        g.FillEllipse(gradientBrush2, gradientRect2);
                    }
                    else
                        g.FillEllipse(gradientBrush1, gradientRect1);
                }




                /////SVETLÁ VPREDU/////
                if (Svetlo != 0)
                {
                    int svetloY = 0, svetloX1 = 0, svetloX2 = 0, sirkaSvetla = 0, vyskaSvetla = 0;
                    if (Svetlo == 1)
                    {
                        svetloX1 = 560;
                        svetloX2 = 125;
                        svetloY = 275;
                        sirkaSvetla = vyskaSvetla = 150;
                    }
                    else if (Svetlo == 2)
                    {
                        svetloX1 = 535;
                        svetloX2 = 100;
                        svetloY = 250;
                        sirkaSvetla = vyskaSvetla = 200;
                    }

                    RectangleF gradientRect1 = new RectangleF(svetloX1, svetloY, sirkaSvetla, vyskaSvetla);
                    RectangleF gradientRect2 = new RectangleF(svetloX2, svetloY, sirkaSvetla, vyskaSvetla);

                    GraphicsPath gradientPath1 = new GraphicsPath();
                    gradientPath1.AddEllipse(gradientRect1);
                    PathGradientBrush gradientBrush1 = new PathGradientBrush(gradientPath1);

                    GraphicsPath gradientPath2 = new GraphicsPath();
                    gradientPath2.AddEllipse(gradientRect2);
                    PathGradientBrush gradientBrush2 = new PathGradientBrush(gradientPath2);

                    gradientBrush1.CenterColor = gradientBrush2.CenterColor = Color.Yellow;

                    Color[] farba = { Color.FromArgb(0, Color.Yellow), Color.FromArgb(32, Color.Yellow) };

                    gradientBrush1.SurroundColors = gradientBrush2.SurroundColors = farba;

                    g.FillEllipse(gradientBrush1, gradientRect1);
                    g.FillEllipse(gradientBrush2, gradientRect2);
                }
            }
            // Nastavenie novej bitovej mapy pre prictureBox7
            pictureBox7.Image = novyObrazok;
        }


        private void UpdateImage_Back()//VYKRESLENIE SVEIEL VZADU
        {

            Bitmap povodnyObrazok = new Bitmap("Images/auto_zadok.png");
            Bitmap novyObrazok = new Bitmap(povodnyObrazok.Width, povodnyObrazok.Height);

            using (Graphics g = Graphics.FromImage(novyObrazok))
            {
                g.DrawImage(povodnyObrazok, new Rectangle(0, 0, novyObrazok.Width, novyObrazok.Height));

                ///////SMEROVKY VZADU//////
                if (hodnotaSmerovky != 0)
                {
                    int svetloX1 = 0, svetloX2 = 0;
                    if (hodnotaSmerovky == 1)
                        svetloX1 = 600;
                    else if (hodnotaSmerovky == -1)
                        svetloX1 = 90;
                    if (hodnotaSmerovky == 2)
                    {
                        svetloX1 = 90;
                        svetloX2 = 600;
                    }

                    int sirkaSvetla = 100, vyskaSvetla = 100; // Width and Height of the light
                    int svetloY = 230; // Y position of the light

                    RectangleF gradientRect1 = new RectangleF(svetloX1, svetloY, sirkaSvetla, vyskaSvetla);
                    RectangleF gradientRect2 = new RectangleF(svetloX2, svetloY, sirkaSvetla, vyskaSvetla);

                    GraphicsPath gradientPath1 = new GraphicsPath();
                    gradientPath1.AddEllipse(gradientRect1);
                    PathGradientBrush gradientBrush1 = new PathGradientBrush(gradientPath1);

                    GraphicsPath gradientPath2 = new GraphicsPath();
                    gradientPath2.AddEllipse(gradientRect2);
                    PathGradientBrush gradientBrush2 = new PathGradientBrush(gradientPath2);

                    gradientBrush1.CenterColor = gradientBrush2.CenterColor = Color.Orange;

                    Color[] farba = { Color.FromArgb(0, Color.Orange), Color.FromArgb(32, Color.Orange) };

                    gradientBrush1.SurroundColors = gradientBrush2.SurroundColors = farba;

                    if (hodnotaSmerovky == 2)
                    {
                        g.FillEllipse(gradientBrush1, gradientRect1);
                        g.FillEllipse(gradientBrush2, gradientRect2);
                    }
                    else
                        g.FillEllipse(gradientBrush1, gradientRect1);
                }


                //////SVETLÁ VZADU////

                if (Svetlo != 0)
                {
                    int svetloX1 = 565;
                    int svetloX2 = 105;
                    int svetloY1 = 170;
                    int sirkaSvetla = 115, vyskaSvetla = 115;

                    RectangleF gradientRect1 = new RectangleF(svetloX1, svetloY1, sirkaSvetla, vyskaSvetla);
                    RectangleF gradientRect2 = new RectangleF(svetloX2, svetloY1, sirkaSvetla, vyskaSvetla);

                    GraphicsPath gradientPath1 = new GraphicsPath();
                    gradientPath1.AddEllipse(gradientRect1);
                    PathGradientBrush gradientBrush1 = new PathGradientBrush(gradientPath1);

                    GraphicsPath gradientPath2 = new GraphicsPath();
                    gradientPath2.AddEllipse(gradientRect2);
                    PathGradientBrush gradientBrush2 = new PathGradientBrush(gradientPath2);

                    gradientBrush1.CenterColor = gradientBrush2.CenterColor = Color.Red;

                    Color[] farba = { Color.FromArgb(0, Color.Red), Color.FromArgb(32, Color.Red) };

                    gradientBrush1.SurroundColors = gradientBrush2.SurroundColors = farba;

                    if (Brzda != true)
                    {
                        g.FillEllipse(gradientBrush1, gradientRect1);
                        g.FillEllipse(gradientBrush2, gradientRect2);
                    }

                }

                /////BRZDY/////
                if (Brzda == true)
                {
                    int svetloX1 = 550;
                    int svetloX2 = 90;
                    int svetloX3 = (svetloX1 + svetloX2) / 2;
                    int svetloY1 = 155;
                    int svetloY2 = 100;
                    int sirkaSvetla = 145, vyskaSvetla = 145;

                    RectangleF gradientRect1 = new RectangleF(svetloX1, svetloY1, sirkaSvetla, vyskaSvetla);
                    RectangleF gradientRect2 = new RectangleF(svetloX2, svetloY1, sirkaSvetla, vyskaSvetla);
                    RectangleF gradientRect3 = new RectangleF(svetloX3, svetloY2, sirkaSvetla, vyskaSvetla);

                    GraphicsPath gradientPath1 = new GraphicsPath();
                    gradientPath1.AddEllipse(gradientRect1);
                    PathGradientBrush gradientBrush1 = new PathGradientBrush(gradientPath1);

                    GraphicsPath gradientPath2 = new GraphicsPath();
                    gradientPath2.AddEllipse(gradientRect2);
                    PathGradientBrush gradientBrush2 = new PathGradientBrush(gradientPath2);

                    GraphicsPath gradientPath3 = new GraphicsPath();
                    gradientPath3.AddEllipse(gradientRect3);
                    PathGradientBrush gradientBrush3 = new PathGradientBrush(gradientPath3);

                    gradientBrush1.CenterColor = gradientBrush2.CenterColor = gradientBrush3.CenterColor = Color.Red;

                    Color[] farba = { Color.FromArgb(0, Color.Red), Color.FromArgb(32, Color.Red) };

                    gradientBrush1.SurroundColors = gradientBrush2.SurroundColors = gradientBrush3.SurroundColors = farba;

                    g.FillEllipse(gradientBrush1, gradientRect1);
                    g.FillEllipse(gradientBrush2, gradientRect2);
                    g.FillEllipse(gradientBrush3, gradientRect3);
                }

                // Nastavenie novej bitovej mapy pre prictureBox8
                pictureBox8.Image = novyObrazok;
            }
        }




        private void pictureBox5_Paint(object sender, PaintEventArgs e)//VYKRESLENIE OVLÁDANIA RÝCHLOSTI VENTILÁTORA
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int stredX = pictureBox5.Width / 2;
            int stredY = pictureBox5.Height / 2;
            int Radius = pictureBox5.Height / 2 - 10;
            int dlzkaIhly = Radius - 20;

            Pen pen1 = new Pen(Color.Black, 2);
            Pen pen2 = new Pen(Color.White, 2);

            SolidBrush brush1 = new SolidBrush(Color.Gray);
            int kruhRadius = 6;
            int kruhX = stredX - kruhRadius * 5;
            int kruhY = stredY - kruhRadius * 5;

            g.FillEllipse(brush1, kruhX, kruhY, kruhRadius * 10, kruhRadius * 10);

            for (int i = -4; i <= 0; i++)
            {
                int uhol = i * 35;
                int dlzkaPera = 7;
                if (i % 5 == 0)
                {
                    dlzkaPera = 7;
                }
                int x1 = stredX + (int)((Radius - 10) * Math.Sin(uhol * Math.PI / 180));
                int y1 = stredY - (int)((Radius - 10) * Math.Cos(uhol * Math.PI / 180));
                int x2 = stredX + (int)(((Radius - 10) - dlzkaPera) * Math.Sin(uhol * Math.PI / 180));
                int y2 = stredY - (int)(((Radius - 10) - dlzkaPera) * Math.Cos(uhol * Math.PI / 180));

                string number = (i + 4).ToString(); // Určenie čísiel na prvku pre ovládnie ventilátora
                int numberX = stredX - 6 + (int)((Radius - 2) * Math.Sin(uhol * Math.PI / 180));
                int numberY = stredY - 6 - (int)((Radius - 2) * Math.Cos(uhol * Math.PI / 180));
                g.DrawString(number, new Font("Arial", 8), Brushes.Black, numberX, numberY);

                g.DrawLine(pen1, x1, y1, x2, y2);


            }

            int uholStupne = (int)(ventilator * 315.0 / 180.0 - 140.0);
            int x01 = stredX + (int)(0.6 * dlzkaIhly * Math.Sin(uholStupne * Math.PI / 180));
            int y01 = stredY - (int)(0.6 * dlzkaIhly * Math.Cos(uholStupne * Math.PI / 180));
            int x02 = stredX + (int)(0.9 * dlzkaIhly * Math.Sin(uholStupne * Math.PI / 180));
            int y02 = stredY - (int)(0.9 * dlzkaIhly * Math.Cos(uholStupne * Math.PI / 180));
            g.DrawLine(pen2, x01, y01, x02, y02);
        }


        private void pictureBox5_MouseDown(object sender, MouseEventArgs e)//SLEDOVANIE POZÍCIE MYŠI
        {
            jeOtacany = true;
            poslednaPoziciaMysi = e.Location;
        }


        private void pictureBox5_MouseMove(object sender, MouseEventArgs e)
        {

            if (jeOtacany)
            {
                int dx = e.X - poslednaPoziciaMysi.X;
                int dy = e.Y - poslednaPoziciaMysi.Y;
                double novyUhol = (ventilatorUhol) + -dy;
                novyUhol = (novyUhol / 20) * 20;
                if (novyUhol < 0) novyUhol = 0;
                if (novyUhol > 100) novyUhol = 100;
                ventilatorUhol = novyUhol;

                // Mapovanie výstupného rozsahu s veľkosťou 20
                if (ventilatorUhol >= 0 && ventilatorUhol < 10)
                    ventilator = 0;
                if (ventilatorUhol > 10 && ventilatorUhol < 30)
                    ventilator = 20;
                if (ventilatorUhol > 30 && ventilatorUhol < 50)
                    ventilator = 40;
                if (ventilatorUhol > 50 && ventilatorUhol < 70)
                    ventilator = 60;
                if (ventilatorUhol > 70 && ventilatorUhol <= 100)
                    ventilator = 80;

                poslednaPoziciaMysi = e.Location;
                pictureBox5.Invalidate(); // redraw the PictureBox
            }
        }


        private void pictureBox5_MouseUp(object sender, MouseEventArgs e)
        {
            jeOtacany = false;
        }




        private void pictureBox6_Paint(object sender, PaintEventArgs e)//VYKRESLENIE OVLÁDANIA TEPLOTY KÚRENIA
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int stredX = pictureBox5.Width / 2;
            int stredY = pictureBox5.Height / 2;
            int Radius = pictureBox5.Height / 2 - 10;
            int dlzkaIhly = Radius - 20;

            Pen pen1 = new Pen(Color.Black, 2);
            Pen pen2 = new Pen(Color.White, 2);

            SolidBrush brush1 = new SolidBrush(Color.Gray);
            int kruhRadius = 6;
            int kruhX = stredX - kruhRadius * 5;
            int kruhY = stredY - kruhRadius * 5;

            g.FillEllipse(brush1, kruhX, kruhY, kruhRadius * 10, kruhRadius * 10);

            for (int i = -4; i <= 0; i++)
            {
                int uhol = i * 35;
                int dlzkaPera = 7;
                if (i % 5 == 0)
                {
                    dlzkaPera = 7;
                }
                int x1 = stredX + (int)((Radius - 10) * Math.Sin(uhol * Math.PI / 180));
                int y1 = stredY - (int)((Radius - 10) * Math.Cos(uhol * Math.PI / 180));
                int x2 = stredX + (int)(((Radius - 10) - dlzkaPera) * Math.Sin(uhol * Math.PI / 180));
                int y2 = stredY - (int)(((Radius - 10) - dlzkaPera) * Math.Cos(uhol * Math.PI / 180));

                if (i == -4)
                {
                    g.DrawArc((new Pen(Color.DarkBlue, 5)), new Rectangle(stredX - Radius + 12, stredY - Radius + 12, (Radius - 12) * 2, (Radius - 12) * 2), 130, 35);
                }

                if (i == -3)
                {
                    g.DrawArc((new Pen(Color.Blue, 5)), new Rectangle(stredX - Radius + 12, stredY - Radius + 12, (Radius - 12) * 2, (Radius - 12) * 2), 165, 35);
                }

                if (i == -2)
                {
                    g.DrawArc((new Pen(Color.Red, 5)), new Rectangle(stredX - Radius + 12, stredY - Radius + 12, (Radius - 12) * 2, (Radius - 12) * 2), 200, 35);
                }

                if (i == -1)
                {
                    g.DrawArc((new Pen(Color.DarkRed, 5)), new Rectangle(stredX - Radius + 12, stredY - Radius + 12, (Radius - 12) * 2, (Radius - 12) * 2), 235, 35);
                }

                g.DrawLine(pen1, x1, y1, x2, y2);


            }

            int uholStupne = (int)(teplota * 315.0 / 180.0 - 140.0);
            int x01 = stredX + (int)(0.6 * dlzkaIhly * Math.Sin(uholStupne * Math.PI / 180));
            int y01 = stredY - (int)(0.6 * dlzkaIhly * Math.Cos(uholStupne * Math.PI / 180));
            int x02 = stredX + (int)(0.9 * dlzkaIhly * Math.Sin(uholStupne * Math.PI / 180));
            int y02 = stredY - (int)(0.9 * dlzkaIhly * Math.Cos(uholStupne * Math.PI / 180));
            g.DrawLine(pen2, x01, y01, x02, y02);
        }


        private void pictureBox6_MouseMove(object sender, MouseEventArgs e)//SLEDOVANIE POZÍCIE MYŠI
        {
            if (jeOtacany)
            {
                //int dx = e.X - poslednaPoziciaMysi.X;
                int dy = e.Y - poslednaPoziciaMysi.Y;
                double novyUhol = (teplotaUhol) + -dy;
                novyUhol = (novyUhol / 20) * 20;
                if (novyUhol < 0) novyUhol = 0;
                if (novyUhol > 100) novyUhol = 100;
                teplotaUhol = novyUhol;

                if (teplotaUhol >= 0 && teplotaUhol < 10)
                    teplota = 0;
                if (teplotaUhol > 10 && teplotaUhol < 30)
                    teplota = 20;
                if (teplotaUhol > 30 && teplotaUhol < 50)
                    teplota = 40;
                if (teplotaUhol > 50 && teplotaUhol < 70)
                    teplota = 60;
                if (teplotaUhol > 70 && teplotaUhol <= 100)
                    teplota = 80;

                poslednaPoziciaMysi = e.Location;
                pictureBox6.Invalidate(); // Prekreslenie pictureBox6
            }
        }


        private void pictureBox6_MouseDown(object sender, MouseEventArgs e)
        {
            jeOtacany = true;
            poslednaPoziciaMysi = e.Location;
        }


        private void pictureBox6_MouseUp(object sender, MouseEventArgs e)
        {
            jeOtacany = false;
        }



        private void timer3_Tick(object sender, EventArgs e)//ČASOVAČ NA ZMENU TEPLOTY INTERIÉRU S INTERVALOM 100ms
        {
            if (bateria != 0)
            {
                switch (teplota)
                {
                    case 0:
                        zmenaTeploty0 = -5;
                        break;
                    case 20:
                        zmenaTeploty0 = -2;
                        break;
                    case 40:
                        zmenaTeploty0 = 0;
                        break;
                    case 60:
                        zmenaTeploty0 = 2;
                        break;
                    case 80:
                        zmenaTeploty0 = 5;
                        break;
                }

                switch (ventilator)
                {
                    case 0:
                        zmenaTeploty = 0;
                        break;
                    case 20:
                        zmenaTeploty = zmenaTeploty0 / 2;
                        break;
                    case 40:
                        zmenaTeploty = zmenaTeploty0;
                        break;
                    case 60:
                        zmenaTeploty = zmenaTeploty0 * 2;
                        break;
                    case 80:
                        zmenaTeploty = zmenaTeploty0 * 3;
                        break;
                }

                if (teplotaKabiny >= 20 && teplotaKabiny <= 35)
                {
                    if (zmenaTeploty >= 0)
                    {
                        if (teplotaMotora >= 35 && teplotaMotora < 40)
                            teplotaKabiny += zmenaTeploty / 5000;
                        else if (teplotaMotora >= 40 && teplotaMotora < 50)
                            teplotaKabiny += zmenaTeploty / 2000;
                        else if (teplotaMotora >= 50 && teplotaMotora < 60)
                            teplotaKabiny += zmenaTeploty / 1500;
                        else if (teplotaMotora >= 60 && teplotaMotora < 70)
                            teplotaKabiny += zmenaTeploty / 1000;
                        else if (teplotaMotora >= 70 && teplotaMotora < 80)
                            teplotaKabiny += zmenaTeploty / 750;
                        else if (teplotaMotora >= 80 && teplotaMotora <= 90)
                            teplotaKabiny += zmenaTeploty / 500;
                    }
                    else
                        teplotaKabiny += zmenaTeploty / 1000;

                    if (ventilator == 0)
                        teplotaKabiny -= 0.0001;

                    // Uistenie, že teplota neklesne pod 20 stupňov alebo nad 35 stupňov
                    if (teplotaKabiny < 20)
                        teplotaKabiny = 20;
                    if (teplotaKabiny > 35)
                        teplotaKabiny = 35;

                }
            }
        }



        //↓↓↓ MOŽNOSTI MENUSTRIP
        private void nabitieBatérieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bateria = 10000;
        }


        private void natankovaťToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hladina_nadrze = 100000;
        }


        private void ukončiťToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}