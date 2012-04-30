using System;
using FubuMVC.Core.Registration.ObjectGraph;

namespace FubuMVC.Core.View.Attachment
{
    public class ProfileViewToken : IViewToken
    {
        private readonly IViewToken _view;
        private readonly string _filteredName;

        public ProfileViewToken(IViewToken view, string filteredName)
        {
            _view = view;
            _filteredName = filteredName;
        }

        public IViewToken View
        {
            get { return _view; }
        }

        public Type ViewType
        {
            get { return _view.ViewType; }
        }

        public Type ViewModel
        {
            get { return _view.ViewModel; }
        }

        public string Name()
        {
            return _filteredName;
        }

        public string Namespace
        {
            get { return _view.Namespace; }
        }

        public ObjectDef ToViewFactoryObjectDef()
        {
            throw new NotSupportedException();
        }
    }
}