using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Spine_Library
{
    /// <summary>
    /// Acess to some basic drawing functions
    /// </summary>
    public abstract class DrawFunctions
    {
        /// <summary>
        /// draws a line between two vectors
        /// </summary>
        /// <param name="drawTex">A blank texture to draw from</param>
        /// <param name="batch">The SpriteBatch to draw to</param>
        /// <param name="width">The width of the line</param>
        /// <param name="color">The color to draw in</param>
        /// <param name="point1">The point to draw from</param>
        /// <param name="point2">The point to draw to</param>
        public static void drawLine(Texture2D drawTex, SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            batch.Draw(drawTex, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }

        /// <summary>
        /// draws a line from a vector with a length and direction
        /// </summary>
        /// <param name="drawTex">A blank texture to draw from</param>
        /// <param name="batch">The SpriteBatch to draw to</param>
        /// <param name="width">The width of the line</param>
        /// <param name="color">The color to draw in</param>
        /// <param name="point1">The orgin point of the line</param>
        /// <param name="length">The length of the line from the orgin</param>
        /// <param name="angle">The angle in RADIANS from to orgin to draw line</param>
        public static void drawLine(Texture2D drawTex, SpriteBatch batch, float width, Color color, Vector2 point1, int length, float angle)
        {
            batch.Draw(drawTex, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }

        /// <summary>
        /// draws an arrow with the base at vector1 and the tip at vector2
        /// </summary>
        /// <param name="drawTex">A blank texture to draw from</param>
        /// <param name="batch">The SpriteBatch to draw to</param>
        /// <param name="lineWidth">The width of the line to draw</param>
        /// <param name="color">The color of the arrow</param>
        /// <param name="point1">The orgin point of the arrow</param>
        /// <param name="point2">The point of the arrow</param>
        public static void drawArrow(Texture2D drawTex, SpriteBatch batch, float lineWidth, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            drawLine(drawTex, batch, lineWidth, color, point1, point2);
            batch.Draw(drawTex, point2, null, color, angle - (float)(Math.PI / 1.15), Vector2.Zero, new Vector2(length / 10, lineWidth), SpriteEffects.None, 0);
            batch.Draw(drawTex, point2, null, color, angle + (float)(Math.PI / 1.15), Vector2.Zero, new Vector2(length / 10, lineWidth), SpriteEffects.None, 0);
        }

        /// <summary>
        /// draws the rectangle from an XNA rectangle
        /// </summary>
        /// <param name="drawTex">A blank texture to draw from</param>
        /// <param name="batch">the SpriteBatch to draw to</param>
        /// <param name="lineWidth">The width of the lines to draw</param>
        /// <param name="color">The color to draw in</param>
        /// <param name="rect">The rectangle to draw</param>
        public static void drawRectangle(Texture2D drawTex, SpriteBatch batch, float lineWidth, Color color, Rectangle rect)
        {
            Vector2 point1 = new Vector2(rect.X, rect.Y);
            Vector2 point2 = new Vector2(rect.X + rect.Width, rect.Y);
            Vector2 point3 = new Vector2(rect.X, rect.Y + rect.Height);
            Vector2 point4 = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

            drawLine(drawTex, batch, lineWidth, color, point1, point2);
            drawLine(drawTex, batch, lineWidth, color, point1, point3);
            drawLine(drawTex, batch, lineWidth, color, point2, point4);
            drawLine(drawTex, batch, lineWidth, color, new Vector2(point3.X - lineWidth, point3.Y), point4);
        }

        /// <summary>
        /// Draws a rectangle between two vectors
        /// </summary>
        /// <param name="drawTex">A blank texture to draw width</param>
        /// <param name="batch">The SpriteBatch to draw from</param>
        /// <param name="lineWidth">The width of the lines to draw</param>
        /// <param name="color">The color or the rectangle</param>
        /// <param name="vector1">The upper-left corner of the rectangle</param>
        /// <param name="vector2">The lower-right corner of the rectangle</param>
        public static void drawRectangle(Texture2D drawTex, SpriteBatch batch, float lineWidth, Color color, Vector2 vector1, Vector2 vector2)
        {
            Vector2 point1 = vector1;
            Vector2 point2 = vector2;
            Vector2 point3 = new Vector2(vector1.X, vector2.Y);
            Vector2 point4 = new Vector2(vector2.X, vector1.Y);

            drawLine(drawTex, batch, lineWidth, color, point1, point4);
            drawLine(drawTex, batch, lineWidth, color, point1, point3);
            drawLine(drawTex, batch, lineWidth, color, point2, point4);
            drawLine(drawTex, batch, lineWidth, color, new Vector2(vector1.X - lineWidth, vector2.Y),
                new Vector2(vector2.X + lineWidth, vector2.Y));
        }

        /// <summary>
        /// Draws a rectangle to the spritebatch from the vector,
        /// length, and width
        /// </summary>
        /// <param name="drawTex">A blank texture to draw width</param>
        /// <param name="batch">The SpiteBatch to draw to</param>
        /// <param name="lineWidth"> The width of the lines to draw</param>
        /// <param name="color">The color of the rectangle</param>
        /// <param name="vector1">The point of orgin for the rectangle</param>
        /// <param name="width">The rectangle's width</param>
        /// <param name="height">The rectangle's height</param>
        public static void drawRectangle(Texture2D drawTex, SpriteBatch batch, float lineWidth, Color color, Vector2 vector1, int width, int height)
        {
            Vector2 point1 = vector1; // top left
            Vector2 point2 = new Vector2(vector1.X + width, vector1.Y + height); // bottom right
            Vector2 point3 = new Vector2(vector1.X, vector1.Y + height); // bottom left
            Vector2 point4 = new Vector2(vector1.X + width, vector1.Y); // top right

            drawLine(drawTex, batch, lineWidth, color, point1, point4);
            drawLine(drawTex, batch, lineWidth, color, point1, point3);
            drawLine(drawTex, batch, lineWidth, color, point2, point4);
            drawLine(drawTex, batch, lineWidth, color, new Vector2(vector1.X - lineWidth, vector1.Y + height), 
                new Vector2(vector1.X + width + lineWidth, vector1.Y + height));
        }

        /// <summary>
        /// Draw a circle from a center and a radius
        /// </summary>
        /// <param name="drawTex">A blank texture to draw from</param>
        /// <param name="batch">The SpriteBatch to draw to</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="stepping">The number of line segments to draw</param>
        /// <param name="radius">The circle's radius</param>
        /// <param name="color">The color to draw in</param>
        public static void drawCircle(Texture2D drawTex, SpriteBatch batch, Color color, Vector2 center, int stepping, int radius)
        {
            //figure out the difference
            double increment = (Math.PI * 2) / stepping;

            //render
            double angle = 0;
            for (int i = 0; i < stepping; i++)
            {
                //draw outline
                drawLine(drawTex, batch, 2F, color, extraMath.calculateVector(center, angle, radius),
                    extraMath.calculateVector(center, angle + increment, radius));
                angle += increment;
            }
        }

        /// <summary>
        /// Draws a slightly buggy triangle
        /// </summary>
        /// <param name="drawTex">A blank texture to use</param>
        /// <param name="batch">The SpriteBatch to draw to</param>
        /// <param name="drawColor">The color to draw in</param>
        /// <param name="lineWidth">The width of the lines to draw</param>
        /// <param name="point1">The top point of the triangle</param>
        /// <param name="point2">The lower left point of the triangle</param>
        /// <param name="point3">The lower right point of the triangle</param>
        public static void drawTriangle(Texture2D drawTex, SpriteBatch batch, Color drawColor, float lineWidth, Vector2 point1, Vector2 point2, Vector2 point3)
        {
                 //P1\\
                //    \\
               //      \\ 
              //        \\
             //P2========P3
            drawLine(drawTex, batch, lineWidth, drawColor, point1, point2);
            drawLine(drawTex, batch, lineWidth, drawColor, point2, point3);
            drawLine(drawTex, batch, lineWidth, drawColor, point3, point1);
        }

        //public static void drawTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Color color)
        //{
        //    int xx = 0, yy = 0;
        //    for (int x = rect.X; x < rect.X + rect.Width; x += 0)
        //    {
        //        for (int y = rect.Y; y < rect.Y + rect.Height; y += 0)
        //        {
        //            Rectangle tempRect = new Rectangle(x, y, tex.Width, tex.Height);
        //            if (x + tex.Width < rect.X + rect.Width & y + tex.Height < rect.Y + rect.Height)
        //                batch.Draw(tex, tempRect, color);
        //            else
        //            {
        //                Rectangle tempSourceRect = new Rectangle(0, 0, rect.Width - (tex.Width * xx), rect.Height - (tex.Height * yy));
        //                Rectangle drawRect = new Rectangle(x, y, tempSourceRect.Width, tempSourceRect.Height);
        //                batch.Draw(tex, drawRect, tempSourceRect, color);
        //            }
        //            y += tex.Height;
        //        }
        //        x += tex.Width;
        //        xx += 1;
        //    }
        //}

        //public static void drawTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Vector2 offSet, Color color)
        //{
        //    rect.X -= (int)offSet.X;
        //    rect.Y -= (int)offSet.Y;
        //    int xx = 0, yy = 0;
        //    for (int x = rect.X; x < rect.X + rect.Width; x += tex.Width)
        //    {
        //        for (int y = rect.Y; y < rect.Y + rect.Height; y += tex.Height)
        //        {
        //            Rectangle tempRect = new Rectangle(x, y, tex.Width, tex.Height);
        //            if (x + tex.Width < rect.X + rect.Width & y + tex.Height < rect.Y + rect.Height)
        //                batch.Draw(tex, tempRect, color);
        //            else
        //            {
        //                Rectangle tempSourceRect = new Rectangle(0, 0, rect.Width - (tex.Width * xx), rect.Height - (tex.Height * yy));
        //                Rectangle drawRect = new Rectangle(x, y, tempSourceRect.Width, tempSourceRect.Height);
        //                batch.Draw(tex, drawRect, tempSourceRect, color);
        //            }
        //        }
        //        xx++;
        //    }

        //    //rect.X -= (int)offSet.X;
        //    //rect.Y -= (int)offSet.Y;
        //    //int xx = (int)Math.Floor((decimal)(rect.Width / tex.Width));
        //    //int sx = (rect.Width / tex.Width) - xx;
        //    //int yy = (int)Math.Floor((decimal)(rect.Height / tex.Height));
        //    //int sy = (rect.Height / tex.Height) - yy;

        //    //for (int x = 0; x < rect.Width; x += tex.Width)
        //    //{
        //    //    for (int y = 0; y < rect.Height; y += tex.Height)
        //    //    {
        //    //        int rx = rect.X + x;
        //    //        int ry = rect.Y + y;
        //    //        if (x + tex.Width < rect.Width & y + tex.Height < rect.Height)
        //    //        {
        //    //            batch.Draw(tex, new Rectangle(rx, ry, tex.Width, tex.Height), color);
        //    //        }
        //    //        else
        //    //            if (x + tex.Width >= rect.Width & y + tex.Height <= rect.Height)
        //    //            {
        //    //                batch.Draw(tex, new Rectangle(rx, ry, (x + tex.Width) - rect.Width, tex.Height),
        //    //                    new Rectangle(rx, ry, (x + tex.Width) - rect.Width, tex.Height), color);
        //    //            }
        //    //            else
        //    //                if (x + tex.Width <= rect.Width & y + tex.Height >= rect.Height)
        //    //                {
        //    //                    batch.Draw(tex, new Rectangle(rx, ry, tex.Width, (y + tex.Height) - rect.Height),
        //    //                        new Rectangle(rx, ry, tex.Width, (y + tex.Height) - rect.Height), color);
        //    //                }
        //    //        //        else
        //    //        //            if (x + tex.Width >= rect.Width & y + tex.Height >= rect.Height)
        //    //        //            {
        //    //        //                batch.Draw(tex, new Rectangle(rx, ry, (x + tex.Width) - rect.Width, (y + tex.Height) - rect.Height),
        //    //        //                    new Rectangle(rx, ry, (x + tex.Width) - rect.Width, (y + tex.Height) - rect.Height), color);
        //    //        //            }
        //    //    }
        //    //}
        //}
    }

    /// <summary>
    /// Acess to some custom Math functions
    /// </summary>
    public abstract class extraMath
    {
        /// <summary>
        /// Calculates the vector at the end of the given line
        /// from another vector
        /// </summary>
        /// <param name="vector">The orgin of the line</param>
        /// <param name="angle">The angle in RADIANS from the line</param>
        /// <param name="length">The length of the line</param>
        /// <returns>Offset Vector</returns>
        public static Vector2 calculateVector(Vector2 vector, double angle, double length)
        {
            Vector2 returnVect = new Vector2(vector.X + (float)((Math.Cos(angle - Math.PI) * length) - Math.PI), vector.Y + (float)(-(Math.Sin(angle - Math.PI) * length)));
            return returnVect;
        }

        /// <summary>
        /// Calculates the angle between two vectors
        /// </summary>
        /// <param name="point1">The orgin vector to calculate from</param>
        /// <param name="point2">The vector to calculate to</param>
        /// <returns>Angle in radians</returns>
        public static double findAngle(Vector2 point1, Vector2 point2)
        {
            double angle = Math.Atan2(point1.Y - point2.Y, point2.X - point1.X);
            return angle;
        }

        /// <summary>
        /// Uses the midpoint displacement algorithm to return
        /// an array of values
        /// </summary>
        /// <param name="h">The smoothing level to use (higher = rougher)</param>
        /// <param name="baseVal">The base value to generate around</param>
        /// <param name="length">The length of the array (must be a multiple of 2)</param>
        /// <returns>A integer heightmap array</returns>
        public static int[] MidpointDisplacement(int h, int baseVal, int length)
        {
            //arguments:
            //
            //argument0 = room width
            //
            //argument1 = height change variable
            //
            //argument2 = land id
            //
            int[] output = new int[length + 1];
            Random RandNum = new Random();

            for (int xx = 0; xx <= length; xx++)
            {
                output[xx] = baseVal;
            }
            output[0] = baseVal;

            //generate values
            for (int rep = 2; rep < length; rep *= 2)
            {
                for (int i = 1; i <= rep; i += 1)
                {

                    int x1 = (length / rep) * (i - 1);
                    int x2 = (length / rep) * i;
                    int avg = (output[x1] + output[x2]) / 2;
                    int Rand = RandNum.Next(-h, h);
                    output[(x1 + x2) / 2] = avg + (Rand);
                }
                h /= 2;
            }

            //returns array
            return output;
        }

        /// <summary>
        /// Returns how many degree change there should be given
        /// a speed relative to circumfrence and a height from
        /// the orgin point
        /// </summary>
        /// <param name="radius">The distance from the center point</param>
        /// <param name="speed">The speed in pixels to rotate around the orgin</param>
        /// <returns>The angle change in RADIANS</returns>
        public static double findCircumfenceAngleChange(double radius, double speed)
        {
            //determine value of n
            double n = Math.Acos((Math.Pow(speed, 2) - 2 * Math.Pow(radius, 2)) / (-2 * Math.Pow(radius, 2)));
            //make sure n is non NAN
            if (n == Double.NaN)
            {
                //set the return to be -1
                n = -1;
                //throw an exception
                throw (new ArithmeticException("Number is NAN!"));
            }
            //return n
            return n;
        }

        /// <summary>
        /// Determines the altitude from a given eliptical orbit
        /// and an angle in RADIANS
        /// </summary>
        /// <param name="length">The length of the elliptical orbit</param>
        /// <param name="width">The width of the elliptical orbit</param>
        /// <param name="theta">The angle in RADIANS from the orgin</param>
        /// <returns>The altitude at the given point</returns>
        public static double getAltitudeFromCenteredOrbit(double length, double width, double theta)
        {
            return length * width / (Math.Sqrt(Math.Pow((length * Math.Cos(theta)), 2) + Math.Pow((width * Math.Sin(theta)), 2)));
        }

        /// <summary>
        /// Determines the altitude from a given eliptical orbit
        /// and an angle in RADIANS
        /// </summary>
        /// <param name="length">The length of the elliptical orbit</param>
        /// <param name="width">The width of the elliptical orbit</param>
        /// <param name="offset">The offset along the length</param>
        /// <param name="theta">The angle in RADIANS from the orgin</param>
        /// <param name="angleOffset">The angle in RADIANS that the orbit is rotated by</param>
        /// <returns>The altitude at the given point</returns>
        public static double getAltitudeFromOffsetOrbit(double length, double width, double offset, double theta, double angleOffset)
        {
            theta -= angleOffset;
            return ((length * width) / Math.Sqrt(Math.Pow(width * Math.Cos(theta), 2) + Math.Pow(length * Math.Sin(theta), 2)));
        }

        /// <summary>
        /// Maps the givan value from one number range to another
        /// </summary>
        /// <param name="lowVal">The low value in the orgin number range</param>
        /// <param name="highVal">The high value in the orgin number range<</param>
        /// <param name="newLowVal">The low value in the new number range<</param>
        /// <param name="newHighVal">The high value in the new number range<</param>
        /// <param name="value">The value to map</param>
        /// <returns>The value mapped to the new range</returns>
        public static double map(double lowVal, double highVal, double newLowVal, double newHighVal, double value)
        {
            double range = newHighVal - newLowVal;
            double oldRange = highVal - lowVal;
            double multiplier = range / oldRange;
            return newLowVal + ((value - lowVal) * multiplier);

        }

        /// <summary>
        /// Returns the angle in RADIANS that is 90° to
        /// the angle from the orgin to the point
        /// </summary>
        /// <param name="orgin">The orgin point</param>
        /// <param name="relativePoint">The point to calculate angle for</param>
        /// <returns>Angle in RADIANS that is 90° from the angle to orgin</returns>
        public static double getDrawAngle(Vector2 orgin, Vector2 relativePoint)
        {
            return -findAngle(orgin, relativePoint) + Math.PI / 2;
        }
    }

    /// <summary>
    /// Handles all the items to be used by entities, inventories,
    /// etc...
    /// </summary>
    public class ItemHandler
    {
        ItemBase[] itemList;
        int itemCount = 0;

        public ItemHandler(int listSize)
        {
            itemList = new ItemBase[listSize];
        }

        /// <summary>
        /// Adds an item to the list
        /// </summary>
        /// <param name="item">The item to add (casted to an ItemBase)</param>
        /// <returns>The ID of the item, -1 if unsucessfull</returns>
        public int addItemToList(ItemBase item)
        {
            int id = getFirstOpenID();
            if (id != -1)
            {
                item.identifier = id;
                itemList[id] = item;
                itemCount++;
                return id;
            }
            return -1;
        }

        /// <summary>
        /// Removes an item from the list with the given ID
        /// </summary>
        /// <param name="id">The ID of the item to remove</param>
        /// <returns>True if the remove was sucessfull</returns>
        public bool removeItemFromList(int id)
        {
            try
            {
                if (itemList[id] != null)
                {
                    itemList[id] = null;
                    itemCount--;
                    return true;
                }
                return false;
            }
            catch (IndexOutOfRangeException)
            {
            }
            return false;
        }

        /// <summary>
        /// Removes an item from the list with the given name
        /// </summary>
        /// <param name="name">The name of the item to remove</param>
        /// <returns>True if the remove was sucessfull</returns>
        public bool removeItemFromList(string name)
        {
            try
            {
                for (int i = 0; i < itemList.Length; i++)
                {
                    if (itemList[i] != null)
                    {
                        if (itemList[i].name == name)
                        {
                            itemList[i] = null;
                            itemCount--;
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (IndexOutOfRangeException)
            {
            }
            return false;
        }

        /// <summary>
        /// returns the first available ID
        /// </summary>
        /// <returns>First open ID</returns>
        private int getFirstOpenID()
        {
            for (int i = 0; i < itemList.Length; i++)
                if (itemList[i] == null)
                    return i;
            return -1;
        }

        /// <summary>
        /// Gets the number of items that the itmeHandler is 
        /// holding
        /// </summary>
        /// <returns>ItemCount</returns>
        public int getItemCount()
        {
            return itemCount;
        }

        /// <summary>
        /// Gets a string of all the item names seperated by newlines
        /// </summary>
        /// <returns>List of names</returns>
        public string getCombinedNames()
        {
            string r = "";
            foreach (ItemBase i in itemList)
            {
                if (i != null)
                {
                    r += i.name + "\n";
                }
            }
            return r;
        }

        /// <summary>
        /// Holds the important values and functions for
        /// items
        /// </summary>
        public class ItemBase
        {
            public Texture2D myTexture;
            public int identifier;
            public double metaData;
            public string name;
            public Event onUseRClick, onUseLClick, onMetaDataEmpty;

            public ItemBase(Texture2D texture, string name, Event onUseRClick, Event onUseLClick, Event onMetaDataEmpty)
            {
                this.myTexture = texture;
                this.name = name;
                this.onUseRClick = onUseRClick;
                this.onUseLClick = onUseLClick;
                this.onMetaDataEmpty = onMetaDataEmpty;
            }
        }
    }

    /// <summary>
    /// Handles the calculation of the framerate
    /// </summary>
    public class FPSHandler
    {
        int framesInSecond = 0, FPS = 0;
        double timer = 0;

        /// <summary>
        /// Handles the FPS counter for the draw event
        /// </summary>
        /// <param name="gameTime">The current GameTime</param>
        public void onDraw(GameTime gameTime)
        {
            framesInSecond++;
            timer += gameTime.ElapsedGameTime.Milliseconds;

            if (timer >= 1000)
            {
                timer = 0;
                FPS = framesInSecond;
                framesInSecond = 0;
            }
        }

        /// <summary>
        /// Returns the framerate as coounted by this object
        /// </summary>
        /// <returns>Framerate</returns>
        public int getFrameRate()
        {
            return FPS;
        }

        /// <summary>
        /// Gets a multiplier that concides with 60 FPS
        /// </summary>
        /// <returns></returns>
        public double getCommonDiff()
        {
            //return 60D / FPS;
            return 1D;
        }
    }

    /// <summary>
    /// Holders for events. Custom events inherit from this
    /// base
    /// </summary>
    public abstract class Event
    {
        public static void doAction()
        {
        }

        public static int doActionInt()
        {
            return -1;
        }

        public static float doActionFloat()
        {
            return -1F;
        }

        public static double doActionDouble()
        {
            return -1D;
        }

        public static ItemHandler.ItemBase doActionItem()
        {
            return null;
        }
    }

    /// <summary>
    /// Handles inventories for players or other things
    /// </summary>
    public class Inventory
    {
        public ItemHandler.ItemBase[] items;
        public Inventory(int slotCount)
        {
        }

        private class Item
        {
            short stackSize;
            ItemHandler.ItemBase BaseItem;

            public Item(ItemHandler.ItemBase BaseItem, short stackSize)
            {
                this.BaseItem = BaseItem;
                this.stackSize = stackSize;
            }

            public short getItemCount()
            {
                return stackSize;
            }
        }
    }

    /// <summary>
    /// Watches a key for a keypress. Note that to make sure action does not 
    /// repeat, use "keywatcher.wasPressed = false;" after action
    /// </summary>
    public class KeyWatcher
    {
        Keys key;
        List<Keys> keys;
        List<keyState> keyStates = new List<keyState>();
        bool isKeyDown = false;
        public bool wasPressed = false;
        byte type = 0;

        /// <summary>
        /// Create a new keywatcher
        /// </summary>
        /// <param name="key">The key to watch</param>
        public KeyWatcher(Keys key)
        {
            this.key = key;
        }

        /// <summary>
        /// Create a new keywatcher that uses multiple keys for one action
        /// or each individual key for that action. Ex: Ctrl + S, + / Add
        /// </summary>
        /// <param name="key">The keys to watch</param>
        /// <param name="all">True if all keys need to be pressed at once</param>
        public KeyWatcher(List<Keys> keys, bool all)
        {
            this.keys = keys;
            if (all)
                this.type = 1;
            else
                this.type = 2;
            foreach (Keys k in keys)
            {
                keyStates.Add(new keyState(k));
            }
        }

        /// <summary>
        /// Updates the key watcher
        /// </summary>
        public void update()
        {
            switch (type)
            {
                #region single key type
                case 0:
                    if (Keyboard.GetState().IsKeyDown(key))
                    {
                        if (!isKeyDown)
                        {
                            wasPressed = true;
                            isKeyDown = true;
                        }
                        else
                        {
                            wasPressed = false;
                        }
                    }
                    else
                    {
                        isKeyDown = false;
                    }
                    break;
                #endregion

                #region all down type
                case 1:
                    int count = 0;
                    foreach (Keys k in keys)
                    {
                        if (Keyboard.GetState().IsKeyDown(k))
                            count ++;
                    }

                    if (count == keys.Count)
                    {
                        if (!isKeyDown)
                        {
                            wasPressed = true;
                            isKeyDown = true;
                        }
                        else
                        {
                            wasPressed = false;
                        }
                    }
                    else
                    {
                        isKeyDown = false;
                    }
                    break;
                #endregion

                #region multiple type
                case 2:
                    bool temp2 = false;
                    foreach (keyState k in keyStates)
                    {
                        k.update();
                        if (k.isKeyDown)
                            temp2 = true;
                    }

                    if (temp2)
                    {
                        if (!isKeyDown)
                        {
                            wasPressed = true;
                            isKeyDown = true;
                        }
                        else
                        {
                            wasPressed = false;
                        }
                    }
                    else
                    {
                        isKeyDown = false;
                    }
                    break;
                #endregion
            }
        }

        /// <summary>
        /// Holds values for multiple key types
        /// </summary>
        private class keyState
        {
            public bool isKeyDown = false;
            public bool wasPressed;
            Keys key;

            public keyState(Keys key)
            {
                this.key = key;
            }

            public void update()
            {
                if (Keyboard.GetState().IsKeyDown(key))
                {
                    if (!isKeyDown)
                    {
                        wasPressed = true;
                        isKeyDown = true;
                    }
                    else
                    {
                        wasPressed = false;
                    }
                }
                else
                {
                    isKeyDown = false;
                }
            }
        }
    }

    /// <summary>
    /// Handles menu sliders
    /// </summary>
    public class Slider
    {
        double value = 0;
        SpriteBatch spriteBatch;
        Rectangle rect;
        Texture2D horizontal, vertical;

        /// <summary>
        /// Initializes the slider
        /// </summary>
        /// <param name="value">The initial value of the slider</param>
        /// <param name="spriteBatch">The SpriteBatch to render with</param>
        /// <param name="rect">The rectangle to draw in</param>
        /// <param name="horizontal">The horizontal texture</param>
        /// <param name="vertical">The vertical texture</param>
        public Slider(float value, SpriteBatch spriteBatch, Rectangle rect, Texture2D horizontal, Texture2D vertical)
        {
            this.value = value;
            this.spriteBatch = spriteBatch;
            this.rect = rect;
            this.horizontal = horizontal;
            this.vertical = vertical;
        }

        /// <summary>
        /// Updates the slider
        /// </summary>
        /// <param name="doRender">True is the slider should render</param>
        public void update(bool doRender)
        {
            MouseState m = Mouse.GetState();

            if (m.LeftButton == ButtonState.Pressed)
            {
                if (m.X > rect.X & m.X < rect.X + rect.Width)
                {
                    if (m.Y > rect.Y & m.Y < rect.Y + rect.Height)
                    {
                        value = ((m.X - (double)rect.X) / 100D);
                    }
                }
            }

            if (doRender)
                render();
        }

        /// <summary>
        /// Renders the slider
        /// </summary>
        public void render()
        {
            spriteBatch.Draw(horizontal, new Rectangle(rect.X, rect.Y + rect.Height/2 - (horizontal.Height / 2), rect.Width, horizontal.Height), Color.White);
            spriteBatch.Draw(vertical, new Rectangle(rect.X + (int)(rect.Width * value), rect.Y, vertical.Width, rect.Height), Color.White);
        }

        /// <summary>
        /// Gets the value in the slider, between 0.0F and 1.0F
        /// </summary>
        /// <returns>Value</returns>
        public float getValue() { return (float)value; }

        /// <summary>
        /// Sets the slider to the specified position
        /// </summary>
        /// <param name="value">Position between 0 and 1 to set to</param>
        public void setValue(float value) 
        { 
            this.value = MathHelper.Clamp(value, 0, 1);  
        }

        /// <summary>
        /// Sets the slider's retangle
        /// </summary>
        /// <param name="rect">The new rectangle to use</param>
        public void setRect(Rectangle rect)
        {
            this.rect = rect;
        }
    }

    /// <summary>
    /// Holds some basic values that can be inherited to allow easier
    /// parenting stuff
    /// </summary>
    public class Instance
    {
        public Vector2 position;
        public double speed, direction, HP, metaData;
    }
}
