﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Tobii.Interaction.Wpf;

namespace E.A.T
{
    /// <summary>
    /// Interaction logic for SpellCheck.xaml
    /// </summary>
    public partial class SpellCheckWindow : Window
    {
        private SpellingError spErr;
        private SpellCheck splCheck;

        public SpellCheckWindow()
        {
            InitializeComponent();
        }

        /**
         * Overide of the base OnClosed
         * We disable the activation mode of tobii
         */
        protected override void OnClosed(EventArgs e)
        {
            ((App)Application.Current).Host.Commands.Input.SendActivationModeOff(); ;
            base.OnClosed(e);
        }

        /**
         * Function that start the spelling correction
         */
        public void ToCorrect(SpellingError spErr)
        {
            ((App)Application.Current).Host.Commands.Input.SendActivationModeOn();
            this.spErr = spErr;
            this.suggestions.Items.Clear();
            int i = 0;
            Style style = this.FindResource("ItemList") as Style;

            this.suggestions.ItemContainerStyle = style;
            //Creation of the item list
            foreach (string sugg in this.spErr.Suggestions)
            {
                //Creation of the item
                TextBlock word = new TextBlock();
                word.Text = sugg;
                word.Name = "sugg" + i.ToString();//Id of the suggestion
                word.SetIsActivatable(true);
                word.SetIsTentativeFocusEnabled(true);
                word.SetActivatedCommand(new ItemCommand(this));
                word.FontSize = 18;
                //Add it to the itemlist
                this.suggestions.Items.Add(word);
                i++;
            }
        }

        /**
         * Callback for the item selection by eye
         */
        public void ItemOfList(object sender)
        {
            this.suggestions.SelectedItem = ((ActivatedArgs)sender).Interactor.Element;

        }

        /**
         * When we click on a button. Forward to SpellButton for the handling
         */
        public void ClickButton(object sender, RoutedEventArgs e)
        {
            this.SpellButton(sender, null);
        }

        /**
         * Trigger with the user validate a button on the spelling window with eye
         */
        public void SpellButton(object sender, ActivationRoutedEventArgs e)
        {
            string bt_name = ((Button)sender).Name;
            switch (bt_name)
            {
                case "bt_quit": //Quit the splelling correction
                    this.Close();
                    break;
                case "bt_ignore": //Ignore on go to the next available error
                    //TO DO: check if there is a list of spelling errors
                    this.Close();
                    break;
                case "bt_ok": //Validation of the correction
                    if(this.suggestions.SelectedItem != null)
                    {
                        string choice = ((TextBlock)this.suggestions.SelectedItem).Text;
                        this.spErr.Correct(choice);
                        //TO DO: check if there is a list of spelling errors
                        this.Close();
                    }
                    break;

            }
            
        }

        /**
        * This function detect when a key is press down
        */
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                    ((App)Application.Current).Host.Commands.Input.SendPanningBegin();//active eye panning
                    break;

            }
        }

        /**
         * This function detect when a key is release
         */
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                    ((App)Application.Current).Host.Commands.Input.SendPanningEnd();//disable eye panning
                    break;
                case Key.Space:
                    ((App)Application.Current).Host.Commands.Input.SendActivation();
                    ((App)Application.Current).Host.Commands.Input.SendActivationModeOn();
                    break;

            }
        }
    }

    /**
     * This class is use to put eye interaction on the item list
     */
    public class ItemCommand : ICommand
    {
        private SpellCheckWindow vm;
        public ItemCommand(SpellCheckWindow vm)
        {
            this.vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter)
        {
            vm.ItemOfList(parameter);
        }
    }
}