using CleanArchitecture.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Represents a global app setting stored as key/value.
    /// </summary>
    public class AppSetting : BaseEntity
    {
        public string? KeyName { get; set; }    // unique identifier for setting
        public string? Value { get; set; }      // JSON or plain string
        public string? Description { get; set; }
    }

}
