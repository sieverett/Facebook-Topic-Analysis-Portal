import React, { Component } from 'react';
import { getPost, translate } from '../Common/Data/Actions';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import TextWell from '../Components/Common/Well/TextWell';
import DateWell from '../Components/Common/Well/DateWell';

class PostMessage extends Component {
  render() {
    const { post } = this.props;
    return (
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
    );
  }
}

class PostInformation extends Component {
  state = {translatedPost: {message: 'Translating...'}}

  componentWillMount() {
    // Load the current post when the page is refreshed or loaded.
    const { postId } = this.props.params;
    getPost(postId, (post, errorMessage) => {
      const translatedPost = Object.assign({}, post, {message: 'Translating...'});
      this.setState({post, translatedPost, errorMessage});

      // Then translate it when we get the message.
      translate(post.message, (translation, errorMessage) => {
        const translatedMessage = errorMessage ? 'Can\'t Translate' : translation.result;
        this.setState({translatedPost: Object.assign({}, post, {
          message: translatedMessage
        })})
      });
    });
  }

  translation() {
    return (
      <Panel title="English Translation" className="full-height-panel">
        <span className='display-whitespace'>{this.state.translation.result}</span>
      </Panel>
    )
  }
  
  viewComments = () => window.location.href += '/comments';

  render() {
    const { post, translatedPost, errorMessage } = this.state;
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
    } else if (!post) {
      return <LoadingIndicator />
    }

    return (
      <section>
        <h1 className="page-header">Post Information</h1>
          <div className="row flex">
            <div className="col-md-6"><PostMessage post={post} /></div>
            <div className="col-md-6"><PostMessage post={translatedPost} /></div>
          </div>
          <div className="row">
            <TextWell className="col-md-4" header={post.reactions.summary.total_count} subheader="Reactions" />
            <TextWell className="col-md-4" header={post.comments.summary.total_count} subheader="Comments" onClick={this.viewComments} />
            <TextWell className="col-md-4" header={post.shares.count} subheader="Shares" />
          </div>
          <hr />
          <div className="row">
            <DateWell className="col-md-4" title="Created" date={post.created_time} />
            <DateWell className="col-md-4" title="Last Updated" date={post.created_time} />
            <DateWell className="col-md-4" title="Last Scraped" date={post.lastScraped} />
          </div>
      </section>
    );
  }
}

export default PostInformation;
