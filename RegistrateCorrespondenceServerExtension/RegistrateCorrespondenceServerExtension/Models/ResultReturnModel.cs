using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RegistrateCorrespondenceServerExtension.Models
{
    public class ResultReturnModel
    {
        public bool Result { get; set; }

        public void Initialize(bool res)
        {
            this.Result = res;
        }
    }
}