using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessObject
{
    [Serializable]
    public class OperatorRequestObject
    {
        string shortcode1;
        string shortcode2;
        string shortcode3;
        string shortcode4;
        string shortcode5;
        string shortcode6;
        string shortcode7;
        string shortcode8;
        string shortcode9;
        string shortcode10;

        public string Shortcode1 { get => shortcode1; set => shortcode1 = value; }
        public string Shortcode2 { get => shortcode2; set => shortcode2 = value; }
        public string Shortcode3 { get => shortcode3; set => shortcode3 = value; }
        public string Shortcode4 { get => shortcode4; set => shortcode4 = value; }
        public string Shortcode5 { get => shortcode5; set => shortcode5 = value; }
        public string Shortcode6 { get => shortcode6; set => shortcode6 = value; }
        public string Shortcode7 { get => shortcode7; set => shortcode7 = value; }
        public string Shortcode8 { get => shortcode8; set => shortcode8 = value; }
        public string Shortcode9 { get => shortcode9; set => shortcode9 = value; }
        public string Shortcode10 { get => shortcode10; set => shortcode10 = value; }
    }
}
