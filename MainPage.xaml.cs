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
        private const int DOOR_SWITCH_PIN = 13;
        private const int RESET_BTN_PIN = 26;

        private GpioPin pirInput;
        private GpioPin ledOutput;
        private GpioPin switchInput;
        private GpioPin resetBtnInput;

        //Application STATUS
        private Boolean pirSensorActive = false;
        private Boolean doorOpen = false;
        private uint pirEventCounter = 0;

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
                pirSensorActive = true;
                ledOutput.Write(GpioPinValue.High);
                pirEventCounter++;
            }
            else
            {
                pirSensorActive = false;
                ledOutput.Write(GpioPinValue.Low);
            }

            // Report the change to the GUI (async)
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdatePirGui();
                UpdateEventCounterGui();
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
            if (pirSensorActive)
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

            switchInput = gpio.OpenPin(DOOR_SWITCH_PIN);
            switchInput.SetDriveMode(GpioPinDriveMode.InputPullUp);

            resetBtnInput = gpio.OpenPin(RESET_BTN_PIN);
            resetBtnInput.SetDriveMode(GpioPinDriveMode.InputPullUp);

            ledOutput = gpio.OpenPin(LED_PIN);
            ledOutput.SetDriveMode(GpioPinDriveMode.Output);
            ledOutput.Write(GpioPinValue.Low);

            GpioStatus.Text = "GPIO pin initialized correctly.";
            UpdateEventCounterGui();
            UpdateDoorGui();
            UpdatePirGui();

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

            GpioPinValue resetBtnPinValue = resetBtnInput.Read();
            if (resetBtnPinValue== GpioPinValue.Low)
            {
                //reset event counter
                pirEventCounter = 0;
                UpdateEventCounterGui();
            }
        }

        private void UpdateEventCounterGui()
        {
            PirCouterText.Text = String.Format("Pir event since reset: {0}", pirEventCounter);
        }
    }
}
