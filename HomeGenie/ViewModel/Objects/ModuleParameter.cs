using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeGenie.ViewModel.Objects
{
    public class ModuleParameter : Data
	{
        private string _value;
        //
		public string Name { get; set; }
		public string Value 
        { 
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                SetField(ref _value, value, "Value");
            }
        }
        public string Description { get; set; }
        public DateTime UpdateTime { get; /* protected */ set; }
        public bool NeedsUpdate { get; set; }
        //
        public double ValueIncrement 
        {
            get
            {
                return (this.DecimalValue - this.LastDecimalValue);
            }
        }
        //
        public string LastValue { get; /* protected */ set; }
        public DateTime LastUpdateTime { get; /* protected */ set; }
        //
        public ModuleParameter()
        {
            this.Name = "";
            this.Value = "";
            this.Description = "";
            this.UpdateTime = DateTime.Now;
            //
            this.LastValue = "";
            this.LastUpdateTime = DateTime.Now;
        }


        public double DecimalValue
        {
          get
          {

            double v;
            if (!double.TryParse(this.Value, out v)) v = 0;
            return v;
          }
        }

        public double LastDecimalValue
        {
          get
          {
            double v;
            if (!double.TryParse(this.LastValue, out v)) v = 0;
            return v;
          }
        }

        public bool Is(string name)
        {
            return (this.Name.ToLower() == name.ToLower());
        }
    }

}
