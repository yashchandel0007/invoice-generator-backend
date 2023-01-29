using System;
using System.Collections.Generic;
using System.Text;

namespace Invoice.Generator.Models.DB
{
    public class UserConfig
    {
        public UserConfig()
        {

        }
        public UserConfig(string userID, string firstName, string lastName, string _type)
        {
            this.userID = userID;
            this.firstName = firstName;
            this.lastName = lastName;
            this._type = _type;
        }
        public string userID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public List<int> currencyIDEnabled { get; set; }
        public int defaultCurrencyID { get; set; }
        public int defaultTaxPercent { get; set; }
        public int defaultDiscountPercent { get; set; }
        public string defaultNotes { get; set; }
        public string defaultTnC { get; set; }
        public string defaultCompanyName { get; set; }
        public Defaultcompanyaddress defaultCompanyAddress { get; set; }
        public string DefaultLogo { get; set; }
        public string _type { get; set; }
    }

    public class Defaultcompanyaddress
    {
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public int zipCode { get; set; }
        public string country { get; set; }
    }


}
