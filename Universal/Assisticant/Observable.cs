/**********************************************************************
 * 
 * Update Controls .NET
 * Copyright 2010 Michael L Perry
 * MIT License
 * 
 * http://updatecontrols.net
 * http://www.codeplex.com/updatecontrols/
 * 
 **********************************************************************/

using System;
using System.Diagnostics;
using Assisticant.Fields;

namespace Assisticant
{
	/// <summary>
    /// A sentry that controls an observable field.
	/// </summary>
	/// <threadsafety static="true" instance="true"/>
	/// <remarks>
    /// An observable field is one whose value can be changed externally at
    /// any time. Create one Observable sentry for each observable field in
	/// your object.
	/// </remarks>
	/// <example>A class using Observable sentries.
	/// <code language="C">
	///	public class Contact
	///	{
	///		private string _name = "";
	///		private string _emailAddress = "";
	///		
    ///		private Observable _indName = new Observable();
    ///		private Observable _indEmailAddress = new Independet();
	///
	///		public Contact()
	///		{
	///		}
	///
	///		public string Name
	///		{
	///			get
	///			{
	///				_indName.OnGet();
	///				return _name;
	///			}
	///			set
	///			{
	///				_indName.OnSet();
	///				_name = value;
	///			}
	///		}
	///
	///		public string EmailAddress
	///		{
	///			get
	///			{
	///				_indEmailAddress.OnGet();
	///				return _emailAddress;
	///			}
	///			set
	///			{
	///				_indEmailAddress.OnSet();
	///				_emailAddress = value;
	///			}
	///		}
	///	}
	/// </code>
    /// <code language="VB">
    ///	Public Class Contact
    ///		Private _name As String = ""
    ///		Private _emailAddress As String = ""
    ///
    ///		Private _indName As New Observable()
    ///		Private _indEmailAddress As New Observable()
    ///
    ///		Public Sub New()
    ///		End Sub
    ///
    ///		Public Property Name() As String
    ///			Get
    ///				_indName.OnGet()
    ///				Return _name
    ///			End Get
    ///			Set
    ///				_indName.OnSet()
    ///				_name = value
    ///			End Set
    ///		End Property
    ///
    ///		Public Property EmailAddress() As String
    ///			Get
    ///				_indEmailAddress.OnGet()
    ///				Return _emailAddress
    ///			End Get
    ///			Set
    ///				_indEmailAddress.OnSet()
    ///				_emailAddress = value
    ///			End Set
    ///		End Property
    ///	End Class
    /// </code>
	/// </example>
	public class Observable : Precedent
	{
		/// <summary>
		/// Call this function just before getting the field that this
		/// sentry controls.
		/// </summary>
		/// <remarks>
		/// Any computed fields that are currently updating will depend upon
		/// this field; when the field changes, the computed becomes
		/// out-of-date.
		/// </remarks>
		public virtual void OnGet()
		{
			// Establish dependency between the current update
			// and this field.
			RecordDependent();
		}

		/// <summary>
		/// Call this function just before setting the field that this
		/// sentry controls.
		/// </summary>
		/// <remarks>
		/// Any computed fields that depend upon this field will become
		/// out-of-date.
		/// </remarks>
		public void OnSet()
		{
            // Verify that dependents are not changing observables, as that
			// could be a logical circular dependency.
			if (Computed.GetCurrentUpdate() != null)
                Debug.Assert(false, "An observable was changed while updating a computed.");

            // When an observable field changes,
			// its dependents become out-of-date.
			MakeDependentsOutOfDate();
		}

		/// <summary>Intended for the debugger. Returns a tree of Computeds that 
		/// use this Computed.</summary>
		protected DependentVisualizer UsedBy
		{
			get { return new DependentVisualizer(this); }
		}
	}
}
