import React, { Component } from 'react';
import { getPostScrape } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import TextWell from '../Components/Common/Well/TextWell';
import DateWell from '../Components/Common/Well/DateWell';

class PostScrapeInformation extends Component {
  state = {}

  componentWillMount() {
    // Load the current scrape when the page is refreshed or loaded.
    const { scrapeId } = this.props.params;
    getPostScrape(scrapeId, (scrape, errorMessage) => this.setState({scrape, errorMessage}));
  }

  handleRowSelected = (data, index) => window.location.href = "https://facebook.com/" + data.facebookId;

  render() {
    const { scrape, errorMessage } = this.state;
    const pagesMapping = [{ name: 'Pages', key: path => path.name }];
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />
    } if (!scrape) {
      return <LoadingIndicator />
    } 

    return (
      <section>
        <Panel className="col-md-4" title="Pages" table>
          <DataTable showHeader={false} showIndex={false} minSize={10}
              mapping={pagesMapping} data={scrape.pages}
              onRowSelected={this.handleRowSelected}
          />
        </Panel>
        <div className="col-md-4">
          <TextWell header={scrape.numberOfPosts} subheader="Posts" />
          <DateWell title="From" date={scrape.since} />
          <DateWell title="Until" date={scrape.until} />
        </div>
        <div className="col-md-4">
          <DateWell title="Started" date={scrape.importStart} />
          <DateWell title="Ended" date={scrape.importEnd} />
          <div className="well">
            <h5 className="text-muted">{scrape.id}</h5>
          </div>
        </div>
      </section>
    );
  }
}

export default PostScrapeInformation;
