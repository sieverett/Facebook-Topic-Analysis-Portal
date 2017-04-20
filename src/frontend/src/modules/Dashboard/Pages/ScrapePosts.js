import React, { Component } from 'react';
import { getPostScrapes, scrapePosts } from '../Common/Data/Actions';
import DateRangeForm from '../Components/Common/DateRangeForm';
import Panel from '../Components/Common/Panel';
import PostScrapeHistory from '../Components/ScrapePosts/PostScrapeHistory';

class ScrapePosts extends Component {
  // Load the up-to-date scrape history each time the page is refreshed or loaded.
  componentWillMount = () => this.getScrapes();

  handleScrapeSubmit = (since, until) => this.context.store.dispatch(scrapePosts(since, until));

  getScrapes = (pageNumber, pageSize) => {
    const { storePageNumber, storePageSize } = this.context.store.getState().postScrapes;
    this.context.store.dispatch(getPostScrapes(pageNumber || storePageNumber, pageSize || storePageSize));
  }
  
  handleScrapeSelected = (data, index) => window.location.href += '/' + data.id;

  render() {
    const { postScrapes, errorMessage } = this.context.store.getState();

    return (
      <section>
        <Panel showHeading={false} className="sub-header">
          <DateRangeForm action="Scrape" lowerName="Since" upperName="Until" onSubmit={this.handleScrapeSubmit} />
        </Panel>
        <PostScrapeHistory scrapes={postScrapes} errorMessage={errorMessage}
            onScrapeSelected={this.handleScrapeSelected} onScrapesPaginationChanged={this.getScrapes} />
      </section>
    );
  }
}
ScrapePosts.contextTypes = {store: React.PropTypes.object};

export default ScrapePosts;
