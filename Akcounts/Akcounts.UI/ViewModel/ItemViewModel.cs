using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Akcounts.Domain;
using Akcounts.DataAccess;
using Akcounts.UI.Util;
using System.Text;


namespace Akcounts.UI.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for an Account object.
    /// </summary>
    public class ItemViewModel : WorkspaceViewModel//, IDataErrorInfo
    {

        readonly Item _item;
        readonly Account _account;

  //      bool _isSelected;
 //       RelayCommand _saveCommand;

        public ItemViewModel(Item item, Account account)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            _item = item;
            _account = account;
        }

        #region Item Properties

        public string TransactionDate
        {
            get { return _item.TransactionDate; }
        }

        public bool IsSource
        {
            get { return (_account == _item.Source); }
        }

        public bool IsDestination
        {
            get { return (_account == _item.Destination); }
        }

        public string ValueIn
        {
            get { return (IsDestination ? _item.Value.ToString("#,##0.00") : ""); }
        }

        public string ValueOut
        {
            get { return (IsSource ? _item.Value.ToString("#,##0.00") : ""); }
        }

        public string OtherAccount
        {
            get {
                if (_item.Source == _account) return _item.DestinationName;
                else return _item.SourceName;
                }
        }

        public string Description
        {
            get { return _item.TransactionDesc; }
        }

        public Boolean IsVerfied
        {
            get { return _item.TVerified; }
        }
         


        #endregion // Account Properties


        #region Presentation Properties

        ///// <summary>
        ///// Gets/sets whether this customer is selected in the UI.
        ///// </summary>
        //public bool IsSelected
        //{
        //    get { return _isSelected; }
        //    set
        //    {
        //        if (value == _isSelected)
        //            return;

        //        _isSelected = value;

        //        base.OnPropertyChanged("IsSelected");
        //    }
        //}

        ///// <summary>
        ///// Returns a command that saves the customer.
        ///// </summary>
        //public ICommand SaveCommand
        //{
        //    get
        //    {
        //        if (_saveCommand == null)
        //        {
        //            _saveCommand = new RelayCommand(
        //                param => this.Save(),
        //                param => this.CanSave
        //                );
        //        }
        //        return _saveCommand;
        //    }
        //}

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Saves the customer to the repository.  This method is invoked by the SaveCommand.
        /// </summary>
        //public void Save()
        //{
        //    if (!_customer.IsValid)
        //        throw new InvalidOperationException(Strings.CustomerViewModel_Exception_CannotSave);

        //    if (this.IsNewCustomer)
        //        _customerRepository.AddCustomer(_customer);
            
        //    base.OnPropertyChanged("DisplayName");
        //}

        #endregion // Public Methods

        #region Private Helpers

        ///// <summary>
        ///// Returns true if this customer was created by the user and it has not yet
        ///// been saved to the customer repository.
        ///// </summary>
        //bool IsNewCustomer
        //{
        //    get { return !_customerRepository.ContainsCustomer(_customer); }
        //}

        ///// <summary>
        ///// Returns true if the customer is valid and can be saved.
        ///// </summary>
        //bool CanSave
        //{
        //    get { return String.IsNullOrEmpty(this.ValidateCustomerType()) && _customer.IsValid; }
        //}

        #endregion // Private Helpers

        //#region IDataErrorInfo Members

        //string IDataErrorInfo.Error
        //{
        //    get { return (_customer as IDataErrorInfo).Error; }
        //}

        //string IDataErrorInfo.this[string propertyName]
        //{
        //    get
        //    {
        //        string error = null;

        //        if (propertyName == "CustomerType")
        //        {
        //            // The IsCompany property of the Customer class 
        //            // is Boolean, so it has no concept of being in
        //            // an "unselected" state.  The CustomerViewModel
        //            // class handles this mapping and validation.
        //            error = this.ValidateCustomerType();
        //        }
        //        else
        //        {
        //            error = (_customer as IDataErrorInfo)[propertyName];
        //        }

        //        // Dirty the commands registered with CommandManager,
        //        // such as our Save command, so that they are queried
        //        // to see if they can execute now.
        //        CommandManager.InvalidateRequerySuggested();

        //        return error;
        //    }
        //}

        //string ValidateCustomerType()
        //{
        //    if (this.CustomerType == Strings.CustomerViewModel_CustomerTypeOption_Company ||
        //       this.CustomerType == Strings.CustomerViewModel_CustomerTypeOption_Person)
        //        return null;

        //    return Strings.CustomerViewModel_Error_MissingCustomerType;
        //}

        //#endregion // IDataErrorInfo Members

    }
}
