import React, { Component } from 'react';
import Moment from 'react-moment';
import { getPosts } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import DateRangeForm from '../Components/Common/DateRangeForm';

class Browse extends Component {
  componentWillMount() {
    // Load the up-to-date list of posts each time the page is refreshed or loaded.
    const { pageNumber, pageSize } = this.context.store.getState().posts;
    this.handlePaginationChanged(pageNumber, pageSize);
  }

  handlePaginationChanged = (pageNumber, pageSize) => this.context.store.dispatch(getPosts(pageNumber, pageSize));

  handleRowSelection = (data, index) => window.location.href += '/' + data.id;

  handleBrowse = (since, until) => {
    const { pageNumber, pageSize } = this.context.store.getState().posts;
    this.context.store.dispatch(getPosts(pageNumber, pageSize, since, until));
  }

  render() {
    const { posts, errorMessage } = this.context.store.getState();
    const mapping = [
      { name: 'Page Id',      key: path => path.page.name                                                 },
      { name: 'Created Time', key: path => <Moment format='YYYY-MM-DD HH:mm'>{path.created_time}</Moment> },
      { name: 'Type',         key: path => path.type                                                      },
      { name: 'Message',      key: path => path.message                                                   },
      { name: 'Comments',     key: path => path.comments.summary.total_count                              },
      { name: 'Reactions',    key: path => path.reactions.summary.total_count                             },
      { name: 'Shares',       key: path => path.shares ? path.shares.count : 0                            },
      { name: 'Status Type',  key: path => path.status_type                                               },
      { name: 'Permalink',    key: path => <a href={path.permalink_url}>Facebook</a>          }
    ];

    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />
    } else if (!posts.data) {
      return <LoadingIndicator />
    } 

    return (
      <section>
        <h1 className="page-header">Browse Posts</h1>
        <DateRangeForm action="Browse" lowerName="From" upperName="To" allowEmpty={true} onSubmit={this.handleBrowse} />
        <DataTable mapping={mapping} data={posts.data} pagination={posts}
                   onRowSelected={this.handleRowSelection} onPaginationChanged={this.handlePaginationChanged} />
      </section>
    );
  }
}
Browse.contextTypes = { store: React.PropTypes.object };

export default Browse;
