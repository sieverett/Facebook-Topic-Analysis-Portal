import React, { Component } from 'react';
import Moment from 'react-moment';
import moment from 'moment';
import { getPageScrapes } from '../Common/Data/Actions';
import DataTable from './Common/Data/DataTable';
import ErrorPanel from './Common/ErrorPanel';
import LoadingIndicator from './Common/LoadingIndicator';

class PageScrapeHistory extends Component {
  formatDifference = (now, then) => moment(moment(then).diff(moment(now))).format('mm:ss');

  componentWilMount() {
    // Load the up-to-date scrape history each time the page is refreshed or loaded.
    var pageNumber = this.context.store.getState().pageScrapes.pageNumber;
    var pageSize = this.context.store.getState().pageScrapes.pageSize;
    this.handlePaginationChanged(pageNumber, pageSize);
  }

  handleRowSelection = (data, index) => window.location.href += '/' + data.id;

  handlePaginationChanged = (pageNumber, pageSize) => this.context.store.dispatch(getPageScrapes(pageNumber, pageSize));

  render() {
    const { pageScrapes, errorMessage } = this.context.store.getState();
    const mapping = [
      { name: 'Date',  key: path => <Moment format='YYYY-MM-DD HH:mm'>{path.importStart}</Moment>     },
      { name: 'Pages', key: path => path.pages.length                                                 },
      { name: 'Took',  key: path => this.formatDifference(path.importStart, path.importEnd) + ' mins' }
    ];

    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
    } else if (!pageScrapes.data) {
      return <LoadingIndicator />
    }

    pageScrapes.showPageNumberForm = false;
    pageScrapes.showPageSizeForm = false;

    return <DataTable alwaysShowPaginationForm={false} minSize={12}
                      mapping={mapping} data={pageScrapes.data} pagination={pageScrapes}
                      onRowSelected={this.handleRowSelection} onPaginationChanged={this.handlePaginationChanged} />;
  }
}
PageScrapeHistory.contextTypes = { store: React.PropTypes.object };

export default PageScrapeHistory;
