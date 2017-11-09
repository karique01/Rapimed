using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rapimed
{
    public partial class Splash : ContentPage
    {
        public Splash()
        {
            InitializeComponent();
            //Device.StartTimer(new TimeSpan(0,0,2), cerrar);
        }
    }
}