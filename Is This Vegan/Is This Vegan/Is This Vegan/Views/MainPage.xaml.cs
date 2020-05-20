﻿using System;
using System.ComponentModel;
using Xamarin.Forms;
using Is_This_Vegan.Backend;

namespace Is_This_Vegan.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        // Page.OnAppearing() runs too early, so this is used to 
        // initialize the following class variables in TouchEffectAction method
        bool hasInitializedImageCoords = false;

        // Vote Circle initial coordinates
        Point vote;

        // Scan Circle initial coordinates
        Point scan;

        // Amount either ScanCircle or VoteCircle has been translated from its original position
        double distanceTranslated;

        // Pressed coordinate
        Point? press;

        // The image (scan or vote) to translate
        Image imageToTranslate;

        public MainPage()
        {
            InitializeComponent();
        }

        public void InitializedImageCoords()
        {
            // Vote and Scan X,Y coordinates are relative to the frame, so to get their coordinates
            // relative to the user's device display, we must also get the coordinates and dimensions
            // of the frame these images exist within
            var relativeVoteCoord = new Point(VoteCircle.X, VoteCircle.Y);
            var relativeScanCoord = new Point(ScanCircle.X, ScanCircle.Y);

            // Gets frame size and coordinates
            Point swipeActionFrame = new Point(SwipeActionFrame.X, SwipeActionFrame.Y);
            double swipeActionFrameHeight = SwipeActionFrame.Height;
            double swipeActionFrameWidth = SwipeActionFrame.Width;
            
            
            // Save inital Vote and Scan coordinates
            var temp = swipeActionFrame.Y + relativeVoteCoord.Y;
            vote = new Point(
                (swipeActionFrame.X + relativeVoteCoord.X),
                (swipeActionFrame.Y + relativeVoteCoord.Y)
                );
            scan = new Point(
                (swipeActionFrame.X + relativeScanCoord.X),
                (swipeActionFrame.Y + relativeScanCoord.Y)
                );
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            if (hasInitializedImageCoords is false)
            {
                InitializedImageCoords();
                hasInitializedImageCoords = true;
            }
            // Convert Xamarin.Forms point to pixels
            Point point = args.Location;

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    var result = TouchActionTypePressedHelper(args.Id, args.Type, point);
                    if (result)
                    {
                        press = point;
                    }
                    break;

                case TouchActionType.Moved:
                    if (press.HasValue)
                    {
                        result = TouchActionTypeMovedHelper(args.Id, args.Type, point);
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    result = TouchActionTypeReleasedOrCancelledHelper(args.Id, args.Type, point);
                    if (result)
                    {
                    }
                    Reset();

                    break;
            }
        }

        /// <summary>
        /// Determines which bitmap was pressed
        /// </summary>
        /// <param name="id"> TouchActionEventArgs Id</param>
        /// <param name="type"> TouchActionEventArgs Type </param>
        /// <param name="point"> Coordinates where the user has touched their screen </param>
        /// <returns> True if the user has touched one of our bitmaps currently displayed, otherwise false </returns>
        public bool TouchActionTypePressedHelper(long id, TouchActionType type, Point point)
        {
            var image = PressOnImageHelper(point);
            
            if (image is null)
            {
                return false;
                imageToTranslate = VoteCircle;
                return true;
            }
            else if (image.Equals(ScanCircle))
            {
                imageToTranslate = ScanCircle;
                return true;
            }
            else if (image.Equals(VoteCircle))
            {
                imageToTranslate = VoteCircle;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if a press gesture is on a valid element (Vote circle or Scan circle)
        /// </summary>
        /// <param name="point"> X,Y coordinate of press gesture </param>
        /// <returns> 
        /// Tuple where Item1 is true if the press gesture is valid, false otherwise.
        /// Item2 is the Image object if Item1 is true, otherwise null.
        /// </returns>
        public Image PressOnImageHelper(Point point)
        {
            var voteMinimumX = vote.X;
            var voteMaximumX = vote.X + VoteCircle.Width;
            var scanMinimumX = scan.X;
            var scanMaximumX = scan.X + ScanCircle.Width;

            if(point.X >= voteMinimumX &&   // Possibly touching VoteCircle
                point.X <= voteMaximumX)
            {
                var voteMinimumY = vote.Y;
                var voteMaximiumY = vote.Y + VoteCircle.Height;
                
                if (point.Y >= voteMinimumY && // Is touching Vote circle
                    point.Y <= voteMaximiumY)
                {
                    return VoteCircle;
                }
            }
            else if (point.X >= scanMinimumX && // Possibly touching ScanCircle
                point.X <= scanMaximumX)
            {
                var scanMinimumY = scan.Y;
                var scanMaximiumY = scan.Y + ScanCircle.Height;

                if (point.Y >= scanMinimumY &&  // Is touching scan circle
                    point.Y <= scanMaximiumY)
                {
                    return ScanCircle;
                }
            }
            // Press is not on a swipable object
            return null;
        }

        /// <summary>
        /// Determines which bitmap was moved
        /// </summary>
        /// <param name="id"> TouchActionEventArgs Id</param>
        /// <param name="type"> TouchActionEventArgs Type </param>
        /// <param name="point"> Coordinates where the user has touched their screen </param>
        /// <returns> True if the user has moved one of our bitmaps currently displayed, otherwise false </returns>
        public bool TouchActionTypeMovedHelper(long id, TouchActionType type, Point point)
        {
            var distance = point.X - ((Point)press).X;
            UpdateDistance(distance);
            
            if (imageToTranslate is null) // User didn't press on a valid image
            {
                return false;
            }
            else if (imageToTranslate.Equals(VoteCircle))
            {
                // If touch is swiping right, move Vote circle
                if (distance > 0)
                {
                    imageToTranslate.TranslationX = distance;
                }
                return true;
            }
            else if (imageToTranslate.Equals(ScanCircle))
            {
                // If touch is swiping left, move Scan circle
                if (distance < 0)
                {
                    imageToTranslate.TranslationX = distance;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines which bitmap was released/cancelled
        /// </summary>
        /// <param name="id"> TouchActionEventArgs Id</param>
        /// <param name="type"> TouchActionEventArgs Type </param>
        /// <param name="point"> Coordinates where the user has stopped touching their screen </param>
        /// <returns> True if the user has released one of our bitmaps currently displayed, otherwise false </returns>
        public bool TouchActionTypeReleasedOrCancelledHelper(long id, TouchActionType type, Point point)
        {
            bool result = false;
            // If user didn't press on a valid image, do nothing
            if (ImageToTranslateIsNull())
            {
                return result;
            }
            // If the user pressed on a valid image
            else if (!ImageToTranslateIsNull())
            {
                if (imageToTranslate.Equals(VoteCircle))
                {
                    result = ShouldNavigateToVote(point);
                }
                else if (imageToTranslate.Equals(ScanCircle))
                {
                    result = ShouldNavigateToScan(point);
                }
                return result;
            }
            // Otherwise do nothing
            else
            {
                return result;
            }
        }

        public bool ImageToTranslateIsNull()
        {
            return imageToTranslate is null;
        }

        /// <summary>
        /// Updates class distanceTranslated variable. Is used when resetting circles to their original position.
        /// </summary>
        /// <param name="distance"> Double value representing the directional distance from the circle's original postion</param>
        /// <returns> The double value representing the total number of units the circle has moved </returns>
        public double UpdateDistance(double distance)
        {
            distanceTranslated = distance;
            return distanceTranslated;
        }

        public bool ShouldNavigateToVote(Point point)
        {
            double threshold = this.Width * ((double)2 / (double)3);

            // Navigate to Vote main screen if release in the right-most third of the device display
            if (point.X >= threshold)
            {
                return true;
            }
            // Otherwise remain on page
            return false;
        }

        public bool ShouldNavigateToScan(Point point)
        {
            double threshold = this.Width * ((double)1 / (double)3);

            // Navigate to Scan screen if release in the left-most third of the device display
            if (point.X <= threshold)
            {
                return true;
            }
            // Otherwise remain on page
            return false;
        }

        public bool Reset()
        {
            try
            {
                ResetCirclePosition();
                press = null;
                imageToTranslate = null;
                return true;
            }
            catch (Exception e)
            {
                this.DisplayAlert("Reset Error", "There seems to have been an unusual error. We apologize for any inconvenience.", "Ok");
                return false;
            }
        }

        /// <summary>
        /// Returns swipable object to original position if it has been moved
        /// </summary>
        /// <returns>
        /// True if neither VoteCircle nor ScanCircle has moved, or if successfully returned to original position.
        /// False if imageToTranslate is not null and not a swipable object, and does nothing.
        /// </returns>
        public bool ResetCirclePosition()
        {
            var scantmp = ScanCircle.X;
            var scanoriginal = scan.X;
            var translated = -distanceTranslated;
            var width = SwipeActionFrame.Width;

            if (imageToTranslate is null) // If circle has not been pressed to begin with, do nothing
            {
                return true;
            }
            else if (imageToTranslate.Equals(ScanCircle)) // Return ScanCircle to original position
            {
                ScanCircle.TranslationX = 0;
                return true;
            }
            else if (imageToTranslate.Equals(VoteCircle)) // Return VoteCircle to original position
            {
                VoteCircle.TranslationX = 0;
                return true;
            }
            else // Unforeseen circumstance
            {
                return false;
            }

        }

        public async void Navigate()
        {
            //if (imageToTranslate.Equals(VoteCircle))
            //{
            //    await Navigation.PushAsync(new BitmapDraggingPage(), true);
            //}
            //else if (imageToTranslate.Equals(ScanCircle))
            //{
            //    await Navigation.PushAsync(new BitmapDraggingPage(), true);
            //}
            //else
            //{
            //    return;
            //}
        }

        /// <summary>
        /// Calls appropriate bitmap's ProcessTouchEvent method
        /// </summary>
        /// <param name="id"> TouchActionEventArgs Id </param>
        /// <param name="type"> TouchActionEventArgs Type </param>
        /// <param name="point"> Coordinates where user has touched on their screen </param>
        /// <param name="bitmap"> BitmapIdEnum indicates which bitmap was pressed.</param>
        /// <returns> True if the user has pressed a bitmap that we recognize, false otherwise </returns>
        //public bool ProcessTouchEventHelper(long id, TouchActionType type, Point point, Image image)
        //{
        //    // User touched scan icon
        //    if (bitmap.Equals(BitmapIdsEnum.scan))
        //    {
        //        scan.ProcessTouchEvent(id, type, point);
        //        return true;
        //    }

        //    // User touched vote icon
        //    if (bitmap.Equals(BitmapIdsEnum.vote))
        //    {
        //        vote.ProcessTouchEvent(id, type, point);
        //        return true;
        //    }

        //    return false;
        //}

        async void BitmapDraggingPage_Navigation_Event(object sender, EventArgs e)
        {

            // Page appearance not animated
            //await Navigation.PushAsync(new BitmapDraggingPage(), true);
        }

        async void Splash_Clicked(object sender, EventArgs e)
        {
            // Page appearance not animated
            await Navigation.PushAsync(new SplashPage(), true);
        }


        //private void Black_Clicked(object sender, System.EventArgs e)
        //{
        //    BgColor.BackgroundColor = Color.FromHex("#000000");
        //}

        //private void Green_Clicked(object sender, System.EventArgs e)
        //{
        //    BgColor.BackgroundColor = Color.FromHex("00281A");
        //}

        //private void Blue_Clicked(object sender, System.EventArgs e)
        //{
        //    BgColor.BackgroundColor = Color.FromHex("040930");

        //}

        //private void Grey_Clicked(object sender, System.EventArgs e)
        //{
        //    BgColor.BackgroundColor = Color.FromHex("1a1a1a");

        //}

        //private void Purple_Clicked(object sender, System.EventArgs e)
        //{
        //    BgColor.BackgroundColor = Color.FromHex("16012B");
        //}

        //private void Eggplant_Clicked(object sender, System.EventArgs e)
        //{
        //    BgColor.BackgroundColor = Color.FromHex("18000F");
        //}
    }
}
