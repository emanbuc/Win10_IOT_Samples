// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Blinky
{
    public sealed partial class MainPage : Page
    {
        //GPIO
        private const int PIR_PIN = 5;
        private const int LED_PIN = 6;

        private GpioPin pirInput;
        private GpioPin ledOutput;

        //Application STATUS
        private Boolean movementDetected = false;

        //Timers
        private const int TIMER1_INTERVALL = 500;
        private DispatcherTimer timer1;

        //GUI
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        public MainPage()
        {
            InitializeComponent();

            InitGPIO();

            //Main application timer
            //used for periodic task like update GUI
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(TIMER1_INTERVALL);
            timer1.Tick += Timer_Tick;
            timer1.Start();            
        }

        /// <summary>
        /// PIR Input Value change event handler
        /// </summary>
        /// <param name="sender">GPIO pin</param>
        /// <param name="args">Event Args</param>
        private void PirInput_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                movementDetected = true;
                ledOutput.Write(GpioPinValue.High);
            }
            else
            {
                movementDetected = false;
                ledOutput.Write(GpioPinValue.Low);
            }
        }

        /// <summary>
        /// Read internal state and update GUI
        /// </summary>
        private void UpdateGui()
        {
            if (movementDetected)
            {
                LED.Fill = redBrush;
            }
            else
            {
                LED.Fill = grayBrush;
            }
        }

        /// <summary>
        /// Init GPIO pin in Input/Opuput mode
        /// </summary>
        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pirInput = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            pirInput = gpio.OpenPin(PIR_PIN);
            pirInput.SetDriveMode(GpioPinDriveMode.Input);
            pirInput.ValueChanged += PirInput_ValueChanged;

            ledOutput = gpio.OpenPin(LED_PIN);
            ledOutput.SetDriveMode(GpioPinDriveMode.Output);
            ledOutput.Write(GpioPinValue.Low);

            GpioStatus.Text = "GPIO pin initialized correctly.";

        }

   




        private void Timer_Tick(object sender, object e)
        {
            UpdateGui();
        }
             

    }
}
