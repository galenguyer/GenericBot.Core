using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace UnitConversionLib
{
    public class UnitConversionLibException : Exception
    {

        #region Constructors

        public UnitConversionLibException()
            : base()
        {
        }

        public UnitConversionLibException(string message)
            : base(message)
        {
        }

        public UnitConversionLibException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnitConversionLibException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Static Methods

        public static void ThrowIf(bool condition, string message = "", Exception inner = null)
        {
            if (condition)
                throw new UnitConversionLibException(message, inner);
        }

        public static void Throw(string message = "", Exception inner = null)
        {
            throw new UnitConversionLibException(message, inner);
        }

        public static UnitConversionLibException UnregedScalic
        {
            get
            {
                return new UnitConversionLibException("Unregistered Scalic Unit");
            }
        }

        public static UnitConversionLibException ScaleNonPositive
        {
            get
            {
                return new UnitConversionLibException("No Negative or zero Scale is allowed");
            }
        }

        public static UnitConversionLibException DirectUnitCreated
        {
            get
            {
                return new UnitConversionLibException("No Direct Created Unit Allowed");
            }
        }

        public static UnitConversionLibException InconsistentUnits
        {
            get
            {
                return new UnitConversionLibException("Inconcsistent units detected while converting");
            }
        }

        #endregion

    }

    public class UnitParsingException : UnitConversionLibException
    {
        public UnitParsingException()
        {
        }

        public UnitParsingException(string message)
            : base(message)
        {
        }

        public UnitParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnitParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public int StartCharacter;

        public string UnknownPhraze;
    }
}