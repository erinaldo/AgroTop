﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Agrotop.Form.Models
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="agrotopsf2016")]
	public partial class AgrotopDBDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Definiciones de métodos de extensibilidad
    partial void OnCreated();
    partial void InsertIncorporacionCamiongo(IncorporacionCamiongo instance);
    partial void UpdateIncorporacionCamiongo(IncorporacionCamiongo instance);
    partial void DeleteIncorporacionCamiongo(IncorporacionCamiongo instance);
    #endregion
		
		public AgrotopDBDataContext() : 
				base(global::System.Configuration.ConfigurationManager.ConnectionStrings["agrotopsf2016ConnectionString1"].ConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public AgrotopDBDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AgrotopDBDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AgrotopDBDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AgrotopDBDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<IncorporacionCamiongo> IncorporacionCamiongo
		{
			get
			{
				return this.GetTable<IncorporacionCamiongo>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.IncorporacionCamiongo")]
	public partial class IncorporacionCamiongo : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _IdIncorporacionCamiongo;
		
		private string _RUT;
		
		private string _Nombre;
		
		private string _Email;
		
		private System.DateTime _FechaHoraIns;
		
		private string _IpIns;
		
    #region Definiciones de métodos de extensibilidad
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdIncorporacionCamiongoChanging(int value);
    partial void OnIdIncorporacionCamiongoChanged();
    partial void OnRUTChanging(string value);
    partial void OnRUTChanged();
    partial void OnNombreChanging(string value);
    partial void OnNombreChanged();
    partial void OnEmailChanging(string value);
    partial void OnEmailChanged();
    partial void OnFechaHoraInsChanging(System.DateTime value);
    partial void OnFechaHoraInsChanged();
    partial void OnIpInsChanging(string value);
    partial void OnIpInsChanged();
    #endregion
		
		public IncorporacionCamiongo()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IdIncorporacionCamiongo", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int IdIncorporacionCamiongo
		{
			get
			{
				return this._IdIncorporacionCamiongo;
			}
			set
			{
				if ((this._IdIncorporacionCamiongo != value))
				{
					this.OnIdIncorporacionCamiongoChanging(value);
					this.SendPropertyChanging();
					this._IdIncorporacionCamiongo = value;
					this.SendPropertyChanged("IdIncorporacionCamiongo");
					this.OnIdIncorporacionCamiongoChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RUT", DbType="VarChar(12) NOT NULL", CanBeNull=false)]
		public string RUT
		{
			get
			{
				return this._RUT;
			}
			set
			{
				if ((this._RUT != value))
				{
					this.OnRUTChanging(value);
					this.SendPropertyChanging();
					this._RUT = value;
					this.SendPropertyChanged("RUT");
					this.OnRUTChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Nombre", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string Nombre
		{
			get
			{
				return this._Nombre;
			}
			set
			{
				if ((this._Nombre != value))
				{
					this.OnNombreChanging(value);
					this.SendPropertyChanging();
					this._Nombre = value;
					this.SendPropertyChanged("Nombre");
					this.OnNombreChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Email", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string Email
		{
			get
			{
				return this._Email;
			}
			set
			{
				if ((this._Email != value))
				{
					this.OnEmailChanging(value);
					this.SendPropertyChanging();
					this._Email = value;
					this.SendPropertyChanged("Email");
					this.OnEmailChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FechaHoraIns", DbType="DateTime NOT NULL")]
		public System.DateTime FechaHoraIns
		{
			get
			{
				return this._FechaHoraIns;
			}
			set
			{
				if ((this._FechaHoraIns != value))
				{
					this.OnFechaHoraInsChanging(value);
					this.SendPropertyChanging();
					this._FechaHoraIns = value;
					this.SendPropertyChanged("FechaHoraIns");
					this.OnFechaHoraInsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IpIns", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string IpIns
		{
			get
			{
				return this._IpIns;
			}
			set
			{
				if ((this._IpIns != value))
				{
					this.OnIpInsChanging(value);
					this.SendPropertyChanging();
					this._IpIns = value;
					this.SendPropertyChanged("IpIns");
					this.OnIpInsChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591