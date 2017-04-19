import React, { Component } from 'react';
import Moment from 'react-moment';
import moment from 'moment';
import { getPostScrapes } from '../Common/Data/Actions';
import PagedDataTableBar from './Common/Data/PagedDataTableBar';
import DataTable from './Common/Data/DataTable';
import ErrorPanel from './Common/ErrorPanel';
import LoadingIndicator from './Common/LoadingIndicator';
import Panel from './Common/Panel';

class PostScrapeHistory extends Component {
  formatDifference = (now, then) => moment(moment(then).diff(moment(now))).format('mm:ss');

  // Load the up-to-date scrape history each time the page is refreshed or loaded.
  componentWillMount = () => this.getScrapes();

  getScrapes = (pageNumber, pageSize) => {
    const { storePageNumber, storePageSize } = this.context.store.getState().postScrapes;
    this.context.store.dispatch(getPostScrapes(pageNumber || storePageNumber, pageSize || storePageSize));
  }

  handleRowSelection = (data, index) => window.location.href += '/' + data.id;

  heading = (scrapes) => {
    return <PagedDataTableBar {...scrapes}
              onPageSizeChanged={size => this.getScrapes(null, size)}
              onPageNumberChanged={number => this.getScrapes(number, null)} />;
  }

  table = (scrapes) => {
    const mapping = [
      { name: 'Date',  key: path => <Moment format='YYYY-MM-DD HH:mm'>{path.importStart}</Moment>     },
      { name: 'Since', key: path => <Moment format='YYYY-MM-DD HH:mm'>{path.since}</Moment>           },
      { name: 'Until', key: path => <Moment format='YYYY-MM-DD HH:mm'>{path.until}</Moment>           },
      { name: 'Pages', key: path => path.pages.length                                                 },
      { name: 'Posts', key: path => path.numberOfPosts                                                },
      { name: 'Took',  key: path => this.formatDifference(path.importStart, path.importEnd) + ' mins' }
    ];

    return (
      <Panel showHeading={false} table={true}>
        <DataTable startIndex={scrapes.startItemIndex + 1} minSize={12}
                   mapping={mapping} data={scrapes.data}
                   onRowSelected={this.handleRowSelection}/>
      </Panel>
    );
  }

  render() {
    const { postScrapes, errorMessage } = this.context.store.getState();
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
    } else if (!postScrapes.data) {
      return <LoadingIndicator />
    }

    return (
      <div>
        {this.heading(postScrapes)}
        {this.table(postScrapes)}
      </div>
    );
  }
}
PostScrapeHistory.contextTypes = { store: React.PropTypes.object };

export default PostScrapeHistory;
