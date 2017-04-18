import React, { Component } from 'react';
import Moment from 'react-moment';
import moment from 'moment';
import { getPostScrapes } from '../Common/Data/Actions';
import DataTable from './Common/Data/DataTable';
import ErrorPanel from './Common/ErrorPanel';
import LoadingIndicator from './Common/LoadingIndicator';

class PostScrapeHistory extends Component {
  formatDifference = (now, then) => moment(moment(then).diff(moment(now))).format('mm:ss');

  componentWillMount() {
    // Load the up-to-date scrape history each time the page is refreshed or loaded.
    const pageNumber = this.context.store.getState().postScrapes.pageNumber;
    const pageSize = this.context.store.getState().postScrapes.pageSize;
    this.handlePaginationChanged(pageNumber, pageSize);
  }

  handlePaginationChanged = (pageNumber, pageSize) => this.context.store.dispatch(getPostScrapes(pageNumber, pageSize));

  handleRowSelection = (data, index) => window.location.href += '/' + data.id;

  render() {
    const { postScrapes, errorMessage } = this.context.store.getState();
    const mapping = [
      { name: 'Date',  key: path => <Moment format='YYYY-MM-DD HH:mm'>{path.importStart}</Moment>     },
      { name: 'Since', key: path => <Moment format='YYYY-MM-DD HH:mm'>{path.since}</Moment>           },
      { name: 'Until', key: path => <Moment format='YYYY-MM-DD HH:mm'>{path.until}</Moment>           },
      { name: 'Pages', key: path => path.pages.length                                                 },
      { name: 'Posts', key: path => path.numberOfPosts                                                },
      { name: 'Took',  key: path => this.formatDifference(path.importStart, path.importEnd) + ' mins' }
    ];

    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
    } else if (!postScrapes.data) {
      return <LoadingIndicator />
    }

    return <DataTable title="Scrape History"
                      mapping={mapping} data={postScrapes.data} pagination={postScrapes}
                      onPaginationChanged={this.handlePaginationChanged} onRowSelected={this.handleRowSelection} />
  }
}
PostScrapeHistory.contextTypes = { store: React.PropTypes.object };

export default PostScrapeHistory;
