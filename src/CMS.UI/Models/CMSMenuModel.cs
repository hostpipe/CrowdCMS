using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class CMSMenuModel
    {
        public string Title { get; set; }
        public List<CMSMenuItem> MenuItems { get; set; }
    }

    public class CMSMenuItem
    {
        private bool _isApprove = false;
        private string _approveText = "Requires Approval";
        private bool _isAssociation = false;
        private string _associationText = "Associations";
        private bool _isComment = false;
        private bool _authorizedCommentExists = false;
        private bool _unauthorizedCommentExists = false;
        private string _commentsText = "Show Comments";
        private bool _isDelete = true;
        private string _deleteText = "Delete";
        private bool _isEdit = true;
        private string _editText = "Edit";
        private bool _isEditImages = false;
        private string _editImagesText = "Edit Images";
        private bool _isExpand = false;
        private string _expandText = "Expand";
        private bool _isInfo = false;
        private string _infoText = "Information included";
        private bool _isManage = false;
        private string _manageText = "Manage Domain Links";
        private bool _isMove = false;
        private bool _isPaymentLogosConf = false;
        private string _paymentLogosConfText = "Manage Payments logos";
        private bool _isPreview = false;
        private string _previewText = "Preview";
        private bool _isStock = false;
        private string _stockText = "Stock";
        private bool _isVisibility = false;
        private string _visibilityText = "Turn on/off";
        private bool _isFuturePublish = false;
        private string _publishDate = "";

        public DateTime? Date { get; set; }
        public int MenuItemID { get; set; }
        public List<CMSMenuItem> SubMenuItems { get; set; }
        public string Title { get; set; }
        public string BoldedTitle { get; set; }
        public bool Visible { get; set; }
        public string CssClasses { get; set; }

        public bool IsFuturePublish
        {
            get { return _isFuturePublish; }
            set { this._isFuturePublish = value; }
        }
        public string PublishDateText
        {
            get { return _publishDate; }
            set { this._publishDate = value; }
        }

        public bool IsApprove
        {
            get { return _isApprove; }
            set { this._isApprove = value; }
        }
        public string ApproveText
        {
            get { return _approveText; }
            set { this._approveText = value; }
        }

        public bool IsAssociation
        {
            get { return _isAssociation; }
            set { this._isAssociation = value; }
        }
        public string AssociationText
        {
            get { return _associationText; }
            set { this._associationText = value; }
        }

        public bool IsComment
        {
            get { return _isComment; }
            set { this._isComment = value; }
        }

        public bool AuthorizedCommentExists
        {
            get { return _authorizedCommentExists; }
            set { _authorizedCommentExists = value; }
        }

        public bool UnauthorizedCommentExists
        {
            get { return _unauthorizedCommentExists; }
            set { _unauthorizedCommentExists = value; }
        }

        public string CommentsText
        {
            get { return _commentsText; }
            set { this._commentsText = value; }
        }

        public bool IsDelete
        {
            get { return _isDelete; }
            set { this._isDelete = value; }
        }
        public string DeleteText
        {
            get { return _deleteText; }
            set { this._deleteText = value; }
        }

        public bool IsEdit
        {
            get { return _isEdit; }
            set { this._isEdit = value; }
        }
        public string EditText
        {
            get { return _editText; }
            set { this._editText = value; }
        }

        public bool IsEditImages
        {
            get { return _isEditImages; }
            set { this._isEditImages = value; }
        }
        public string EditImagesText
        {
            get { return _editImagesText; }
            set { this._editImagesText = value; }
        }

        public bool IsExpand
        {
            get { return _isExpand; }
            set { this._isExpand = value; }
        }
        public string ExpandText
        {
            get { return _expandText; }
            set { this._expandText = value; }
        }

        public bool IsInfo
        {
            get { return _isInfo; }
            set { this._isInfo = value; }
        }
        public string InfoText
        {
            get { return _infoText; }
            set { this._infoText = value; }
        }

        public bool IsManage
        {
            get { return _isManage; }
            set { this._isManage = value; }
        }
        public string ManageText
        {
            get { return _manageText; }
            set { this._manageText = value; }
        }

        public bool IsMove
        {
            get { return _isMove; }
            set { this._isMove = value; }
        }
        public string MoveAction { get; set; }

        public bool IsPaymentLogosConf
        {
            get { return _isPaymentLogosConf; }
            set { this._isPaymentLogosConf = value; }
        }
        public string PaymentLogosConfText
        {
            get { return _paymentLogosConfText; }
            set { this._paymentLogosConfText = value; }
        }

        public bool IsPreview
        {
            get { return _isPreview; }
            set { this._isPreview = value; }
        }
        public string PreviewUrl { get; set; }
        public string PreviewText
        {
            get { return _previewText; }
            set { this._previewText = value; }
        }

        public bool IsStock
        {
            get { return _isStock; }
            set { this._isStock = value; }
        }
        public string StockText
        {
            get { return _stockText; }
            set { this._stockText = value; }
        }

        public bool IsVisibility
        {
            get { return _isVisibility; }
            set { this._isVisibility = value; }
        }
        public string VisibilityText
        {
            get { return _visibilityText; }
            set { this._visibilityText = value; }
        }
    }
}