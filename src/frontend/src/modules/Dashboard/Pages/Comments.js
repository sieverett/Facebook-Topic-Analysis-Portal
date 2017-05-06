import React, { Component } from 'react';
import { showDate } from '../Common/Utilities';
import { exportComments, getComments } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import PagedDataTableBar from '../Components/Common/Data/PagedDataTableBar';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import DateRangeForm from '../Components/Common/DateRangeForm';
import PageSelectionList from '../Components/Common/PageSelectionList';

class Comments extends Component {
  state = {}

  // Load the up-to-date list of posts each time the page is refreshed or loaded.
  componentWillMount = () => this.getComments();

  getComments = (newPageNumber, newPageSize, newSince, newUntil, newPages, newOrderingKey, newOrderingDescending) => {
    const { pageNumber, pageSize, sort } = this.context.store.getState().comments;
    const since = newSince || this.state.since;
    const until = newUntil || this.state.until;
    const pages = newPages || this.state.pages;

    const orderingKey = newOrderingDescending === undefined && sort ? sort[0].field : newOrderingKey;
    const descending = newOrderingDescending === undefined && sort ? sort[0].order === 'desc' : newOrderingDescending;
    this.context.store.dispatch(getComments(
      newPageNumber || pageNumber,
      newPageSize || pageSize,
      newSince || since,
      newUntil || until,
      pages,
      orderingKey,
      descending
    ));
    
    this.setState({pages, since, until});
  }

  handleExport = (contentType, since, until) => {
    exportComments(contentType, since, until, this.state.pages, (_, errorMessage) => {});
  }

  handleRowSelection = (data, index) => window.location.href += `/../posts/${data.post.id}`;

  handleOrderingChanged = (orderingKey, descending) => this.getComments(null, null, null, null, orderingKey, descending);

  toggleFilterPages = () => this.setState({filterPagesListVisible: !this.state.filterPagesListVisible});

  handleFilterPages = (pages) => this.getComments(null, null, null, null, pages.map(p => p.facebookId));

  export = () => {
    const extraButtonActions = [
      {title: 'Export as CSV',  onClick: (since, until) => this.handleExport('csv', since, until)  },
      {title: 'Export as JSON', onClick: (since, until) => this.handleExport('json', since, until) }
    ];

    const { since, until } = this.context.store.getState().comments;
    return (
      
      <Panel showHeading={false} className="sub-header">
        <DateRangeForm action="Browse" onSubmit={(since, until) => this.getComments(null, null, since, until)}
                       filterTitle="Filter Pages" onFilterClicked={this.toggleFilterPages} extraButtonActions={extraButtonActions}
                       since={since} lowerName="From" until={until} upperName="To" allowEmpty={true} />
      </Panel>
    );
  }

  heading = (pagination) => {
    return <PagedDataTableBar {...pagination}
              onPageSizeChanged={pageSize => this.getComments(null, pageSize)}
              onPageNumberChanged={pageNumber => this.getComments(pageNumber, null)} />;
  }
  
  table = (comments) => {
    const mapping = [
      {name: 'Page',         key: comment => <a href={`pages/${comment.post.from.id}`}>{comment.post.from.name}</a>                                                            },
      {name: 'From',         key: comment => <a href={`https://facebook.com/${comment.from.id}`}>{comment.from.name}</a>                                                       },
      {name: 'Message',      key: comment => comment.message                                                                                                                   },
      {name: 'Created Time', key: comment => showDate(comment.created_time),                                                                      orderingKey: 'created_time'  },
      {name: 'Likes',        key: comment => <span><strong>{comment.like_count}</strong> <small className="text-muted">Likes</small></span>,      orderingKey: 'like_count'    },
      {name: 'Replies',      key: comment => <span><strong>{comment.comment_count}</strong> <small className="text-muted">Replies</small></span>, orderingKey: 'comment_count' }
    ];

    return (
      <Panel showHeading={false} table={true}>
        <DataTable mapping={mapping} data={comments.data} startIndex={comments.startItemIndex + 1} minSize={12}
                   onRowSelected={this.handleRowSelection}
                   orderingKey={comments.sort[0].field} orderDescending={comments.sort[0].order === 'desc'} onOrderingChanged={this.handleOrderingChanged} />
      </Panel>
    );
  }

  pagesList = () => {
    const pagesListClassName = 'col-md-3 ' + (this.state.filterPagesListVisible ? 'slide-in-visible' : 'slide-in-hidden');
    return <PageSelectionList title="Filter" className={pagesListClassName} onSubmit={this.handleFilterPages} />
  }

  render() {
    const { comments, errorMessage } = this.context.store.getState();
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />
    } else if (!comments.data) {
      return <LoadingIndicator />
    }
    
    const tableContainerClassName = 'table-container ' + (this.state.filterPagesListVisible ? 'col-md-9 ' : 'col-md-12');

    return (
      <div>
        {this.export()}
        <div className="page-filtered-list">
          {this.pagesList()}
          <div className={tableContainerClassName}>
            {this.heading(comments)}
            {this.table(comments)}
          </div>
        </div>
      </div>
    );
  }
}
Comments.contextTypes = {store: React.PropTypes.object};

export default Comments;
