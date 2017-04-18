import React, { Component } from 'react';
import Moment from 'react-moment';
import { getPages, newPage, newPages, editPage, deletePage } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import ToolbarPanel from '../Components/Common/ToolbarPanel';
import AddPageForm from '../Components/AddPageForm';
import ImportPagesForm from '../Components/ImportPagesForm';

class Pages extends Component {
  state = {}

  componentWillMount() {
    // Load the up-to-date list of pages each time the page is refreshed or loaded.
    const pageNumber = this.context.store.getState().pages.pageNumber;
    const pageSize = this.context.store.getState().pages.pageSize;
    this.handlePaginationChanged(pageNumber, pageSize);
  }

  handlePaginationChanged = (pageNumber, pageSize) => this.context.store.dispatch(getPages(pageNumber, pageSize));

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

  handleRowSelected = (data, index) => this.setState({selectedPage: data, editingPage: null});

  viewSelectedPage = () => window.location.href += '/' + this.state.selectedPage.id;

  editSelectedPage = () => this.setState({editingPage: this.state.selectedPage});
  cancelEditingPage = () => this.setState({editingPage: null});

  deleteSelectedPage = () => {
    this.context.store.dispatch(deletePage(this.state.selectedPage.id)).then(this.clearSelectedPage);
  }

  clearSelectedPage = () => this.setState({selectedPage: null, editingPage: null});

  showDate(date, fallback) { return date ? <Moment format='YYYY-MM-DD HH:mm'>{date}</Moment> : fallback; }

  pagesList = () => {
    const { pages, errorMessage } = this.context.store.getState();
    const mapping = [
      { name: 'Name',            key: path => path.name                                         },
      { name: 'ID',              key: path => path.facebookId                                   },
      { name: 'Number Of Likes', key: path => (path.fanCount || 0) + ' Likes'                   },
      { name: 'First Scrape',    key: path => this.showDate(path.firstScrape, 'Never Scraped')  },
      { name: 'Latest Scrape',   key: path => this.showDate(path.latestScrape, 'Never Scraped') },
      { name: 'Added',           key: path => this.showDate(path.created)                       }
    ];

    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />
    } else if (!pages.data) {
      return <LoadingIndicator />
    } 

    pages.showPageSizeForm = false;
    pages.showPageNumberForm = false;
    return <DataTable alwaysShowPaginationForm={false} minSize={12}
                      mapping={mapping} data={pages.data} pagination={pages}
                      onRowSelected={this.handleRowSelected} onPaginationChanged={this.handlePaginationChanged} />;
  }

  selectedPage = () => {
    var editAction;
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
    return (
      <section>
        <h1 className="page-header">Browse Pages</h1>
        <Panel className="col-md-8" title="Pages" table>
          {this.pagesList()}            
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
