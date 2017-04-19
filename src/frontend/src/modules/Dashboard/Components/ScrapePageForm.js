import React, { Component } from 'react';
import { getPages, scrapePages } from '../Common/Data/Actions';
import DataTable from './Common/Data/DataTable';
import ErrorPanel from './Common/ErrorPanel';
import LoadingIndicator from './Common/LoadingIndicator';
import Modal from './Common/Modal';
import Panel, { PanelHeading } from './Common/Panel';

class ScrapePageForm extends Component {
  state = {selectedRows: [], modalId: 'import-pages-modal'}

  componentWillMount() {
    // Load the up-to-date list of pages when 
    this.selectAll();
    this.context.store.dispatch(getPages()).then(this.selectAll);
  }

  handleSubmit = (event) => {
    event.preventDefault();

    var errorMessage = [];
    if (this.state.selectedRows.length === 0) {
      errorMessage.push(<p key='name-empty'>No pages were selected.</p>);
    }

    if (errorMessage.length === 0) {
      this.context.store.dispatch(scrapePages(this.state.selectedRows));
      return true;
    }

    this.setState({'errorMessage': errorMessage});
    window.showModal('#' + this.state.modalId);
  }

  handlePanelClicked = () => this.state.selectedRows.length === 0 ? this.selectAll() : this.setState({selectedRows: []});
  selectAll = () => this.setState({selectedRows: this.context.store.getState().pages.data.map(p => p.id)});

  isRowSelected = (data, index) => this.state.selectedRows.includes(data.id);

  handleRowSelection = (data, index) => {
    if (this.isRowSelected(data)) {
      // Selected: remove from the list of selected rows.
      const rowsWithoutSelection = this.state.selectedRows.filter(p => p !== data.id);
      this.setState({selectedRows: rowsWithoutSelection});
    } else {
      // Not selected: add to the list of selected rows.
      const rowsWithSelection = this.state.selectedRows.concat([data.id]);
      this.setState({selectedRows: rowsWithSelection});
    }
  }

  pageList() {
    const { pages, errorMessage } = this.context.store.getState();
    const mapping = [ 'Name' ];
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
    } else if (!pages.data) {
      return <LoadingIndicator />
    }

    pages.showPageSizeForm = false;
    pages.showPageNumberForm = false;
    return <DataTable alwaysShowPaginationForm={false} minSize={10} showIndex={false} showHeader={false} striped={false}
                      mapping={mapping} data={pages.data} pagination={pages}
                      selectionChecker={this.isRowSelected} onRowSelected={this.handleRowSelection} />
  }

  render() {
    // If nothing is selected, show a "Select All" button. Else show a "Clear" button.
    const panelButtonTitle = this.state.selectedRows.length === 0 ? 'All' : 'Clear';
    const panelHeading = <PanelHeading title="Scrape Pages" buttonTitle={panelButtonTitle} onClick={this.handlePanelClicked} />;

    return (
      <Panel className="col-md-5" table heading={panelHeading}>
        <form onSubmit={this.handleSubmit}>
          <div className="form-group">
            {this.pageList()}
          </div>
          <div className="panel-footer">
            <input type="submit" className="btn btn-primary btn-block" value="Scrape" />
          </div>
        </form>
        <Modal id={this.state.modalId} title="Cannot scrape page">{this.state.errorMessage}</Modal>
      </Panel>
    );
  }
}
ScrapePageForm.contextTypes = { store: React.PropTypes.object };

export default ScrapePageForm;
