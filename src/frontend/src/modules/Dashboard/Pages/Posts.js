import React, { Component } from 'react';
import { showDate } from '../Common/Utilities';
import { exportPosts, getPosts } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import PagedDataTableBar from '../Components/Common/Data/PagedDataTableBar';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import DateRangeForm from '../Components/Common/DateRangeForm';

class Posts extends Component {
  state = {}

  // Load the up-to-date list of posts each time the page is refreshed or loaded.
  componentWillMount = () => this.getPosts();

  getPosts = (newPageNumber, newPageSize, newSince, newUntil, newOrderingKey, newOrderingDescending) => {
    const { pageNumber, pageSize, since, until, ordering } = this.context.store.getState().posts;
    const orderingKey = newOrderingDescending === undefined && ordering ? ordering.key : newOrderingKey;
    const descending = newOrderingDescending === undefined && ordering ? ordering.descending : newOrderingDescending;
    this.context.store.dispatch(getPosts(newPageNumber || pageNumber, newPageSize || pageSize, newSince || since, newUntil || until, orderingKey, descending));
  }
  
  handleExport = (contentType, since, until) => exportPosts(contentType, since, until, (_, errorMessage) => {});

  handleRowSelection = (data, index) => window.location.href += '/' + data.id;

  handleOrderingChanged = (orderingKey, descending) => this.getPosts(null, null, null, null, orderingKey, descending);

  export = () => {
    const { since, until } = this.context.store.getState().posts;
    const extraButtonActions = [
      {title: 'Export as CSV',  onClick: () => this.handleExport('csv', since, until)  },
      {title: 'Export as JSON', onClick: () => this.handleExport('json', since, until) }
    ];

    return (
      <Panel showHeading={false} className="sub-header">
        <DateRangeForm action="Browse" onSubmit={(since, until) => this.getPosts(null, null, since, until)} extraButtonActions={extraButtonActions}
                       since={since} lowerName="From" until={until} upperName="To" allowEmpty={true} />
      </Panel>
    );
  }

  heading = (pagination) => {
    return <PagedDataTableBar {...pagination}
              onPageSizeChanged={pageSize => this.getPosts(null, pageSize)}
              onPageNumberChanged={pageNumber => this.getPosts(pageNumber, null)} />;
  }
  
  table = (posts) => {
    const mapping = [
      {name: 'Page Id',      key: post => post.page.name                                                                        },
      {name: 'Created Time', key: post => showDate(post.created_time),              orderingKey: 'created_time'                 },
      {name: 'Type',         key: post => post.type                                                                             },
      {name: 'Message',      key: post => post.message                                                                          },
      {name: 'Comments',     key: post => post.comments.summary.total_count,        orderingKey: 'comments.summary.total_count' },
      {name: 'Reactions',    key: post => post.reactions.summary.total_count,       orderingKey: 'reactions.summary.total_count'},
      {name: 'Shares',       key: post => post.shares.count,                        orderingKey: 'shares.count'                 },
      {name: 'Status Type',  key: post => post.status_type                                                                      },
      {name: 'Permalink',    key: post => <a href={post.permalink_url}>Facebook</a>                                             }
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
    const { posts, errorMessage } = this.context.store.getState();
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />
    } else if (!posts.data) {
      return <LoadingIndicator />
    }

    return (
      <div>
        {this.export()}
        {this.heading(posts)}
        {this.table(posts)}
      </div>
    );
  }
}
Posts.contextTypes = {store: React.PropTypes.object};

export default Posts;
