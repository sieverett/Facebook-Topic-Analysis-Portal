import React, { Component } from 'react';
import { getPostComments } from '../Common/Data/Actions';
import { showDate } from '../Common/Utilities';
import DataTable from '../Components/Common/Data/DataTable';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';

class PostCommentsInformation extends Component {
  // Load the up-to-date list of comments each time the page is refreshed or loaded.
  componentWillMount = () => this.getComments();

  getComments = (newPageNumber, newPageSize) => {
    const { postId } = this.props.params;
    const { pageNumber, pageSize } = this.context.store.getState().comments;
    this.context.store.dispatch(getPostComments(postId, newPageNumber || pageNumber, newPageSize || pageSize));
  }
  
  handleRowSelection = (data, index) => window.location.href = `https://facebook.com/${data.id}`;

  render() {
    const { comments, errorMessage } = this.context.store.getState();
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
    } else if (!comments.data) {
      return <LoadingIndicator />
    }

    const mapping = [
      {name: 'From',    key: comment => comment.from.name                                                                                  },
      {name: 'Message', key: comment => comment.message                                                                                    },
      {name: 'Likes',   key: comment => <span><strong>{comment.like_count}</strong> <small className="text-muted">Likes</small></span>     },
      {name: 'Replies', key: comment => <span><strong>{comment.comment_count}</strong> <small className="text-muted">Replies</small></span>},
      {name: 'Created', key: comment => showDate(comment.created_time)                                                                     }

    ];

    return (
      <section>
        <h1 className="page-header">Comments</h1>
        <Panel showHeading={false} table={true}>
          <DataTable mapping={mapping} data={comments.data} showIndex={false} minSize={12} onRowSelected={this.handleRowSelection} />
        </Panel>
      </section>
    );
  }
}
PostCommentsInformation.contextTypes = {store: React.PropTypes.object};

export default PostCommentsInformation;
