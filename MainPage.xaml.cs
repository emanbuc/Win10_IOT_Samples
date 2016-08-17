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
        private const int PIR_PIN = 5;
        private const int LED_PIN = 6;
        private GpioPin pirInput;
        private GpioPin ledOutput;
        private Boolean movementDetected = false;

        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        public MainPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            InitGPIO();
            if (pirInput != null)
            {
                timer.Start();
            }
     
        }

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
            if (movementDetected)
            {
                LED.Fill = redBrush;
            }
            else
            {
                LED.Fill = grayBrush;
            }
        }
             

    }
}
