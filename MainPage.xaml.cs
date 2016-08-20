// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Core;
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
        private const int SWITCH_PIN = 13;

        private GpioPin pirInput;
        private GpioPin ledOutput;
        private GpioPin switchInput;

        //Application STATUS
        private Boolean movementDetected = false;
        private Boolean doorOpen = false;

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

            //application timer
            //used for periodic task like check an input values
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(TIMER1_INTERVALL);
            timer1.Tick += Timer1_Tick;
            timer1.Start();            
        }

        /// <summary>
        /// PIR Input Value change event handler
        /// </summary>
        /// <param name="sender">GPIO pin</param>
        /// <param name="args">Event Args</param>
        private void PirInput_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {

            //Update state
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

            // Report the change to the GUI (async)
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdatePirGui();
            });

        }

        /// <summary>
        /// Update Door GUI based on aplication status
        /// </summary>
        private void UpdateDoorGui()
        {
            if (doorOpen)
            {
                DoorStatus.Text = "Door OPEN";
            }
            else
            {
                DoorStatus.Text = "Door CLOSED";
            }
        }

        /// <summary>
        /// Read internal state and update GUI
        /// </summary>
        private void UpdatePirGui()
        {
            if (movementDetected)
            {
                PirStatus.Fill = redBrush;
            }
            else
            {
                PirStatus.Fill = grayBrush;
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

            switchInput = gpio.OpenPin(SWITCH_PIN);
            switchInput.SetDriveMode(GpioPinDriveMode.InputPullUp);
            

            ledOutput = gpio.OpenPin(LED_PIN);
            ledOutput.SetDriveMode(GpioPinDriveMode.Output);
            ledOutput.Write(GpioPinValue.Low);

            GpioStatus.Text = "GPIO pin initialized correctly.";

        }

   




        private void Timer1_Tick(object sender, object e)
        {
            GpioPinValue switchPinValue = switchInput.Read();
            if(switchPinValue == GpioPinValue.High)
            {
                doorOpen = true;
            }
            else
            {
                doorOpen = false;
            }

            // Report the change to the GUI (async)
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateDoorGui();
            });
        }

    }
}
