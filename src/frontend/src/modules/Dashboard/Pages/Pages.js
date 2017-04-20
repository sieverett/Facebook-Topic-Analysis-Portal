import React, { Component } from 'react';
import { getPages, newPage, newPages, editPage, deletePage } from '../Common/Data/Actions';
import Panel from '../Components/Common/Panel';
import ToolbarPanel from '../Components/Common/ToolbarPanel';
import AddPageForm from '../Components/Pages/AddPageForm';
import ImportPagesForm from '../Components/Pages/ImportPagesForm';
import PageList from '../Components/Pages/PageList';

class Pages extends Component {
  state = {}

  // Load the up-to-date list of pages each time the page is refreshed or loaded.
  componentWillMount = () => this.getPages();

  getPages = (newPageNumber, newPageSize) => {
    const { pageNumber, pageSize } = this.context.store.getState().pages;
    this.context.store.dispatch(getPages(newPageNumber || pageNumber, newPageSize || pageSize));
  } 

  handleEditPage = (name, facebookId) => {
    this.context.store.dispatch(editPage(this.state.editingPage.id, name, facebookId)).then(() => {
      this.clearSelectedPage();
      this.setState({clearEditPageForm: true});
    });
  }

  handleAddPage = (name, id) => {
    this.context.store.dispatch(newPage(name , id)).then(() => this.setState({clearAddPageForm: true}));
  }

  handleImportPages = (pages) => {
    this.context.store.dispatch(newPages(pages)).then(() => this.setState({clearImportForm: true}));
  }

  handleCleared = () => this.setState({clearEditPageForm: false, clearAddPageForm: false, clearImportForm: false});

  handlePageSelected = (data, index) => this.setState({selectedPage: data, editingPage: null});

  viewSelectedPage = () => window.location.href += '/' + this.state.selectedPage.id;

  editSelectedPage = () => this.setState({editingPage: this.state.selectedPage});
  cancelEditingPage = () => this.setState({editingPage: null});

  deleteSelectedPage = () => {
    this.context.store.dispatch(deletePage(this.state.selectedPage.id)).then(this.clearSelectedPage);
  }

  clearSelectedPage = () => this.setState({selectedPage: null, editingPage: null});

  selectedPage = () => {
    let editAction;
    if (this.state.editingPage) {
      editAction = { className: 'btn-primary', title: 'Cancel', onClick: this.cancelEditingPage };
    } else {
      editAction = { className: 'btn-primary', title: 'Edit',   onClick: this.editSelectedPage };
    }

    const pageActions = [
      { className: 'btn-default', title: 'View',   onClick: this.viewSelectedPage },
      editAction,
      { className: 'btn-danger',  title: 'Delete', onClick: this.deleteSelectedPage }
    ];

    if (this.state.selectedPage) { 
      return <ToolbarPanel title={this.state.selectedPage.name} actions={pageActions} />;
    }
  }

  editingPage = () => {
    if (this.state.editingPage) {
      return <AddPageForm title="Edit Page" name={this.state.selectedPage.name} facebookId={this.state.selectedPage.facebookId} submitButtonTitle="Save"
                          onSubmit={this.handleEditPage} clear={this.state.clearEditPageForm} onClear={this.handleCleared} />
    }
  }

  addPage = () => {
    return <AddPageForm title="Add Page" submitButtonTitle="Add"
                        onSubmit={this.handleAddPage} clear={this.state.clearAddPageForm} onClear={this.handleCleared} />
  }

  importPages = () => {
    return <ImportPagesForm onSubmit={this.handleImportPages} clear={this.state.clearImportForm} onClear={this.handleCleared} />
  }

  render() {
    const { pages, errorMessage } = this.context.store.getState();

    return (
      <section>
        <h1 className="page-header">Browse Pages</h1>
        <Panel className="col-md-8" title="Pages" table>
          <PageList pages={pages} errorMessage={errorMessage} onRowSelected={this.handlePageSelected} />
        </Panel>
        <section className="col-md-4">
          {this.selectedPage()}
          {this.editingPage()}
          {this.addPage()}            
          {this.importPages()}
        </section>
      </section>
    );
  }
}
Pages.contextTypes = { store: React.PropTypes.object };

export default Pages;
