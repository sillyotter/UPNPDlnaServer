using System;

namespace MediaServer.Web
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	sealed class SoapParameterAttribute : Attribute
	{
		public string Name { get; private set; }

		public SoapParameterAttribute(string name)
		{
			Name = name;
		}
	}
}