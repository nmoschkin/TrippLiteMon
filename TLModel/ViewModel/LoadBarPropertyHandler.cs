using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using DataTools.Scheduler;

namespace TrippLite
{

    public class LoadBarPropertyHandler
    {
        /// <summary>
        /// Creates a new object with the specified get and set minimum and maximum value delegate functions and optional intial value.
        /// </summary>
        /// <param name="getMinFunc">The get minimum delegate.</param>
        /// <param name="setMinFunc">The set minimum delegate.</param>
        /// <param name="getMaxFunc">The get maximum delegate.</param>
        /// <param name="setMaxFunc">The set maximum delegate.</param>
        /// <param name="value">The optional initial value.</param>
        /// <remarks></remarks>
        public LoadBarPropertyHandler(GetMinimumValue getMinFunc, SetMinimumValue setMinFunc, GetMaximumValue getMaxFunc, SetMaximumValue setMaxFunc, double value = 0d)
        {
            setMax = setMaxFunc;
            getMax = getMaxFunc;

            setMin = setMinFunc;
            getMin = getMinFunc;

            this.value = value;
        }

        /// <summary>
        /// Creates a new object with the specified minimum, maximum and initial values.
        /// </summary>
        /// <param name="minVal">Minimum value.</param>
        /// <param name="maxVal">Maximum value.</param>
        /// <param name="value">Default value.</param>
        /// <remarks></remarks>
        public LoadBarPropertyHandler(double minVal, double maxVal, double value)
        {
            getMin = new GetMinimumValue(() => min);
            getMax = new GetMaximumValue(() => max);

            setMin = new SetMinimumValue((v) => min = v);
            setMax = new SetMaximumValue((v) => max = v);

            MinValue = minVal;
            MaxValue = maxVal;

            if (value < minVal || value > maxVal)
            {
                throw new ArgumentOutOfRangeException("Value cannot be less than minVal or greater than maxVal");
            }

            this.value = value;
        }

        private double value;
        private double min;
        private double max;

        public delegate double GetMinimumValue();

        public delegate double GetMaximumValue();

        public delegate void SetMinimumValue(double v);

        public delegate void SetMaximumValue(double v);

        private GetMinimumValue getMin;
        private GetMaximumValue getMax;
        private SetMinimumValue setMin;
        private SetMaximumValue setMax;

        /// <summary>
        /// Creates a new object with the specified minimum and maximum values.
        /// </summary>
        /// <param name="minVal">Minimum value.</param>
        /// <param name="maxVal">Maximum value.</param>
        /// <remarks></remarks>
        public LoadBarPropertyHandler(double minVal, double maxVal) : this(minVal, maxVal, minVal)
        {
        }

        /// <summary>
        /// Gets or sets the minimum load bar value.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double MinValue
        {
            get
            {
                return getMin();
            }
            internal set
            {
                setMin(value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum load bar value.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double MaxValue
        {
            get
            {
                return getMax();
            }
            internal set
            {
                setMax(value);
            }
        }

        /// <summary>
        /// Gets or sets the current load bar value.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double Value
        {
            get
            {
                return value;
            }
            internal set
            {

                // ' in other circumstances, we may want to throw an exception.
                // ' however this is to report the load bar values, and there
                // ' are some circumstances where the load bar value may
                // ' be within an accepted physical threshold (for instance, voltage)
                // ' but beyond the ability of the graphic to produce.
                if (value < MinValue)
                {
                    value = MinValue;
                }
                else if (value > MaxValue)
                {
                    value = MaxValue;
                }

                this.value = value;
            }
        }

        /// <summary>
        /// Handle the load bar value for the given value.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public double HandleLoadBarValue(double v)
        {
            value = v;
            return HandleLoadBarValue();
        }

        /// <summary>
        /// Handle the load bar value for the current value.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public double HandleLoadBarValue()
        {
            double d = MaxValue - MinValue;
            double e = Value - MinValue;

            if (d < 0d)
            {
                throw new InvalidConstraintException("Maximum value is less than minimum value.");
            }

            return e / d;
        }

    }

}
