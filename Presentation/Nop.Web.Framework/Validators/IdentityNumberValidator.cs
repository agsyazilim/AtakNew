// Atilla KayaNopCommerceNop.Web.FrameworkIdentityNumberValidator.cs

using System;
using System.Linq.Expressions;
using FluentValidation.Resources;
using FluentValidation.Validators;

namespace Nop.Web.Framework.Validators
{
    public class IdentityNumberValidator:PropertyValidator
    {
        public IdentityNumberValidator() : base("Identity Number is not valid")
        {
        }
        protected override bool IsValid(PropertyValidatorContext context)
        {
            var ccValue = context.PropertyValue as string;
            if (string.IsNullOrWhiteSpace(ccValue))
                return false;
            bool tcOk = System.Text.RegularExpressions.Regex.Match(ccValue, "^[1-9]{1}[0-9]{10}$").Success;
            bool tcNo = false;
            if (tcOk)    
            {
               tcNo = CheckDigit(ccValue);
            }
            return tcNo;

        }
        private bool CheckDigit(string tcNo) 
        { 
            string tmpStr = tcNo; 
            int sumOfTen = 0; 
            int lastOne = 0; 
            while (true) 
            { 
                if (tmpStr.Length > 1)//ilk 10 basamak mı 
                { 
                    sumOfTen = sumOfTen + Convert.ToInt32(tmpStr.Substring(0, 1)); 
                    tmpStr = tmpStr.Remove(0, 1); 
                } 
                else // son basamak 
                { 
                    lastOne = Convert.ToInt32(tmpStr); 
                    break; 
                } 
            } 

            return sumOfTen%10 == lastOne;//ilk 10 basamak toplamının son basamağı, tcNo soın basamağına eşit olacak kuralını test eder ve döner. 
        } 
    }
}