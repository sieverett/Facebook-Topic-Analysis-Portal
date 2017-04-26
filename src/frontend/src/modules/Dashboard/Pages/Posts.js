import React, { Component } from 'react';
import { showDate } from '../Common/Utilities';
import { exportPosts, getPosts } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import PagedDataTableBar from '../Components/Common/Data/PagedDataTableBar';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import DateRangeForm from '../Components/Common/DateRangeForm';

class Browse extends Component {
  state = {}

  // Load the up-to-date list of posts each time the page is refreshed or loaded.
  componentWillMount = () => this.getPosts(null, null);

  getPosts = (newPageNumber, newPageSize, newSince, newUntil) => {
    const { pageNumber, pageSize, since, until } = this.context.store.getState().posts;
    console.log(newSince);
    this.context.store.dispatch(getPosts(newPageNumber || pageNumber, newPageSize || pageSize, newSince || since, newUntil || until));
  }
  
  handleExportToCSV = (since, until) => exportPosts(since, until, (_, errorMessage) => {});

  handleRowSelection = (data, index) => window.location.href += '/' + data.id;

  export = () => {
    const { since, until } = this.context.store.getState().posts;
    return (
      <Panel showHeading={false} className="sub-header">
        <DateRangeForm action="Browse" onSubmit={(since, until) => this.getPosts(null, null, since, until)}
                       extraButtonAction="Export to CSV" onExtraButtonClicked={this.handleExportToCSV}
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
      { name: 'Page Id',      key: path => path.page.name                            },
      { name: 'Created Time', key: path => showDate(path.created_time)               },
      { name: 'Type',         key: path => path.type                                 },
      { name: 'Message',      key: path => path.message                              },
      { name: 'Comments',     key: path => path.comments.summary.total_count         },
      { name: 'Reactions',    key: path => path.reactions.summary.total_count        },
      { name: 'Shares',       key: path => path.shares ? path.shares.count : 0       },
      { name: 'Status Type',  key: path => path.status_type                          },
      { name: 'Permalink',    key: path => <a href={path.permalink_url}>Facebook</a> }
    ];

    return (
      <Panel showHeading={false} table={true}>
        <DataTable mapping={mapping} data={posts.data} startIndex={posts.startItemIndex + 1} minSize={12}
                   onRowSelected={this.handleRowSelection} />
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
Browse.contextTypes = { store: React.PropTypes.object };

export default Browse;
