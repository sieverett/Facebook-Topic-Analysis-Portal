import React, { Component } from 'react';
import { showDate } from '../Common/Utilities';
import { exportComments, getComments } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import PagedDataTableBar from '../Components/Common/Data/PagedDataTableBar';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import DateRangeForm from '../Components/Common/DateRangeForm';

class Comments extends Component {
  state = {}

  // Load the up-to-date list of posts each time the page is refreshed or loaded.
  componentWillMount = () => this.getComments();

  getComments = (newPageNumber, newPageSize, newSince, newUntil, newOrderingKey, newOrderingDescending) => {
    const { pageNumber, pageSize, since, until, ordering } = this.context.store.getState().comments;
    const orderingKey = newOrderingDescending === undefined && ordering ? ordering.key : newOrderingKey;
    const descending = newOrderingDescending === undefined && ordering ? ordering.descending : newOrderingDescending;
    this.context.store.dispatch(getComments(newPageNumber || pageNumber, newPageSize || pageSize, newSince || since, newUntil || until, orderingKey, descending));
  }
  
  handleExport = (contentType, since, until) => exportComments(contentType, since, until, (_, errorMessage) => {});

  handleRowSelection = (data, index) => window.location.href += '/' + data.id;

  handleOrderingChanged = (orderingKey, descending) => this.getComments(null, null, null, null, orderingKey, descending);

  export = () => {
    const { since, until } = this.context.store.getState().comments;
    const extraButtonActions = [
      {title: 'Export as CSV',  onClick: () => this.handleExport('csv', since, until)  },
      {title: 'Export as JSON', onClick: () => this.handleExport('json', since, until) }
    ];

    return (
      <Panel showHeading={false} className="sub-header">
        <DateRangeForm action="Browse" onSubmit={(since, until) => this.getComments(null, null, since, until)} extraButtonActions={extraButtonActions}
                       since={since} lowerName="From" until={until} upperName="To" allowEmpty={true} />
      </Panel>
    );
  }

  heading = (pagination) => {
    return <PagedDataTableBar {...pagination}
              onPageSizeChanged={pageSize => this.getComments(null, pageSize)}
              onPageNumberChanged={pageNumber => this.getComments(pageNumber, null)} />;
  }
  
  table = (posts) => {
    const mapping = [
      {name: 'Page',         key: comment => comment.post.from.name                                                                                                            },
      {name: 'From',         key: comment => comment.from.name                                                                                                                 },
      {name: 'Message',      key: comment => comment.message                                                                                                                   },
      {name: 'Created Time', key: comment => showDate(comment.created_time),                                                                      orderingKey: 'created_time'  },
      {name: 'Likes',        key: comment => <span><strong>{comment.like_count}</strong> <small className="text-muted">Likes</small></span>,      orderingKey: 'like_count'    },
      {name: 'Replies',      key: comment => <span><strong>{comment.comment_count}</strong> <small className="text-muted">Replies</small></span>, orderingKey: 'comment_count' }
    ];

    return (
      <Panel showHeading={false} table={true}>
        <DataTable mapping={mapping} data={posts.data} startIndex={posts.startItemIndex + 1} minSize={12}
                   onRowSelected={this.handleRowSelection}
                   orderingKey={posts.ordering.key} orderDescending={posts.ordering.descending} onOrderingChanged={this.handleOrderingChanged} />
      </Panel>
    );
  }

  render() {
    const { comments, errorMessage } = this.context.store.getState();
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />
    } else if (!comments.data) {
      return <LoadingIndicator />
    }

    return (
      <div>
        {this.export()}
        {this.heading(comments)}
        {this.table(comments)}
      </div>
    );
  }
}
Comments.contextTypes = {store: React.PropTypes.object};

export default Comments;
