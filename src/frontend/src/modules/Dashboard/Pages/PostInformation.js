import React, { Component } from 'react';
import { getPost, translatePost } from '../Common/Data/Actions';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import TextWell from '../Components/Common/Well/TextWell';
import DateWell from '../Components/Common/Well/DateWell';

class PostInformation extends Component {
  state = {}

  componentWillMount() {
    // Load the current post when the page is refreshed or loaded.
    const { postId } = this.props.params;
    getPost(postId, (post, errorMessage) => {
      this.setState({post, errorMessage});
      // Then translate it when we get the message.
      translatePost(postId, (translation, errorMessage) => {
        if (errorMessage) {
          console.log('Could not translate!');
        } else {
          this.setState({translation});
        }
      });
    });
  }

  render() {
    const { post, errorMessage } = this.state;
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
    } else if (!post) {
      return <LoadingIndicator />
    }

    return (
      <section>
        <h1 className="page-header">Page Information</h1>
        <div className="col-md-4">
          <div className="panel panel-default">
            <div className="panel-heading">
              <strong>{post.page.name}</strong>
              <span className="pull-right">{post.type}</span>
            </div>
            <div className="panel-body"><span className='display-whitespace'>{post.message}</span></div>
            <div className="panel-footer">
              <a href={post.permalink_url}>{post.permalink_url}</a>
            </div>
          </div>
        </div>
        <div className="col-md-4">
          <TextWell header={post.reactions.summary.total_count} subheader="Reactions" />
          <TextWell header={post.comments.summary.total_count} subheader="Comments" />
          <TextWell header={post.shares.count} subheader="Shares" />
        </div>
        <div className="col-md-4">
          <DateWell title="Created" date={post.created_time} />
          <DateWell title="Last Updated" date={post.created_time} />
          <DateWell title="Last Scraped" date={post.lastScraped} />
          <div className="well">
            <h5 className="text-muted">{post.id}</h5>
          </div>
        </div>
      </section>
    );
  }
}

export default PostInformation;
