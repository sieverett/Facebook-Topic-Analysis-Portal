import React, { Component } from 'react';
import { getPageScrape } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import DateWell from '../Components/Common/Well/DateWell';

class PageScrapeInformation extends Component {
  state = {}

  componentWillMount() {
    const { scrapeId } = this.props.params;
    getPageScrape(scrapeId, (page, errorMessage) => this.setState({page, errorMessage}));
  }

  render() {
    const { scrape, errorMessage } = this.state;
    const pagesMapping = [
      { 'name': 'Page',            key: path => path.name                                                          },
      { 'name': 'Number of Likes', key: path => <p>{path.fanCount} <small className="text-muted">Likes</small></p> }
    ];
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />
    } else if (!scrape) {
      return <LoadingIndicator />
    } 

    return (
      <section>
        <Panel className="col-md-6" title="Pages" table>
          <DataTable showHeader={false} showIndex={false}
              mapping={pagesMapping} data={scrape.pages}
              onRowSelected={this.handleRowSelected}
          />
        </Panel>
        <div className="col-md-6">
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

export default PageScrapeInformation;
